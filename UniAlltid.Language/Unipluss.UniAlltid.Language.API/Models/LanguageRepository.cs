using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Dapper;
using static System.String;

namespace UniAlltid.Language.API.Models
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly IDbConnection _connection;

        public LanguageRepository(IDbConnection connection)
        {
            _connection = connection;
   
        }

        public IEnumerable<Translation> Retrieve(string customer, string language)
        {
            if (IsNullOrEmpty(customer) && IsNullOrEmpty(language))
                return GetDefaultValues();

            return GetFilteredValues(customer, language);
        }

        public Dictionary<string, string> RetrieveDictionary(string language, string customer)
        {
            var translations = GetFilteredValues(customer, language);

            var dict = new Dictionary<string, string>();

            foreach (var translation in translations)
            {
                if (!dict.ContainsKey(translation.KeyId))
                    dict.Add(translation.KeyId, translation.Value);
            }

            return dict;
        }

        public void Create(NewTranslation translation)
        {
            if(KeyAlreadyExists(translation.KeyId))
                throw new Exception("Key already exists");

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("insert into t_language");
            sql.AppendLine("values((select max(id) + 1 from t_language), @keyid, 'no', @value, null)");
            sql.AppendLine("insert into t_language");
            sql.AppendLine("values((select max(id) + 1 from t_language), @keyid, 'en', @valueEnglish, null)");

            _connection.Execute(sql.ToString(), new
            {
                keyid = translation.KeyId,
                value = translation.Value,
                valueEnglish = translation.ValueEnglish
            });
        }

        public void CreateOrUpdateSingle(NewSingleTranslation translation)
        {
            StringBuilder sql = new StringBuilder();

            if (KeyAlreadyExists(translation.KeyId))
            {
                sql.AppendLine("update t_language set value = @value");
                sql.AppendLine("where keyId = @keyId and lang = @language");
            }
            else
            {
                sql.AppendLine("insert into t_language");
                sql.AppendLine("values((select max(id) + 1 from t_language), @keyid, @language, @value, null)");
            }

            _connection.Execute(sql.ToString(), new
            {
                value = translation.Value,
                keyId = translation.KeyId,
                language = translation.Language
            });
        }

        public void Update(Translation translation, string selectedCustomer)
        {
            LogChange(translation.KeyId, translation.Lang, translation.Value, selectedCustomer, "Dev");

            StringBuilder sql = new StringBuilder();

            if (IsNullOrEmpty(translation.Customer) && IsNullOrEmpty(selectedCustomer))
            {
                sql.AppendFormat("update t_language set value = @value where id=@id");
                _connection.Execute(sql.ToString(), new { value = translation.Value, id = translation.Id });
            }
            else if (!IsNullOrEmpty(translation.Customer))
            {                          
                sql.AppendLine("select * from t_language where keyid = @keyid and lang = @lang and IsNull(customer, '') = ''");

                Translation defaultEntry =
                    _connection.Query<Translation>(sql.ToString(),
                        new { keyid = translation.KeyId, lang = translation.Lang }).First();

                if (translation.Value == defaultEntry.Value || IsNullOrEmpty(translation.Value))
                {
                    sql = new StringBuilder();
                    sql.AppendLine("delete from t_language where id=@id");
                    _connection.Execute(sql.ToString(), new { id = translation.Id });
                }
                else
                {
                    sql = new StringBuilder();
                    sql.AppendFormat("update t_language set value = @value where id=@id");
                    _connection.Execute(sql.ToString(), new { value = translation.Value, id = translation.Id });
                }
            }
            else
            {
                sql = new StringBuilder();
                sql.AppendLine("insert into t_language");
                sql.AppendLine("values((select max(id) + 1 from t_language), @keyid, @lang, @value, @customer)");

                _connection.Execute(sql.ToString(), new
                {
                    keyid = translation.KeyId,
                    lang = translation.Lang,
                    value = translation.Value,
                    customer = selectedCustomer
                });
            }
        }

        public void UpdateKey(int id, string keyId)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("update t_language set keyId = @keyId");
            sql.AppendLine("where keyId = (select top 1 keyId from t_language where id = @id)");

            _connection.Execute(sql.ToString(), new {id, keyId});
        }

        public void Delete(int id)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("delete from t_language where id=@id");

            _connection.Execute(sql.ToString(), new {id});
        }

        public IEnumerable<Customer> RetrieveCustomers()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from t_customer");

            return _connection.Query<Customer>(sql.ToString());
        }

        public void CreateCustomer(Customer customer)
        {
            StringBuilder sql = new StringBuilder();

            sql.AppendLine("select count(*) from t_customer where id = @id");
            var result = (int)_connection.ExecuteScalar(sql.ToString(), new {id = customer.Id});
            if(result != 0)
                throw new Exception("Customer with ID " + customer.Id + " already exists.");

            sql = new StringBuilder();
            sql.AppendLine("insert into t_customer values(@id, @name)");

            _connection.Execute(sql.ToString(), new {id = customer.Id, name = customer.Name});

        }

        public HttpResponseMessage ExportCSV()
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK);
            result.Content = new StringContent(GetDataToExport());

            result.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
            result.Content.Headers.ContentDisposition.FileName = "translations.csv";

            return result;
        }

        public void UpdateCustomerKeys(IEnumerable<ExternalTranslation> translations, string customer, string updatedBy)
        {
            foreach (var translation in translations)
            {
                if (translation.Norwegian != null)
                    UpdateCustomerKey(translation.KeyId, translation.Norwegian, customer, Language.NO, updatedBy);

                if (translation.English != null)
                    UpdateCustomerKey(translation.KeyId, translation.English, customer, Language.EN, updatedBy);
            }
        }

        public IEnumerable<ExternalTranslation> RetrieveExternalTranslations(IEnumerable<string> keyIds, string customer = "")
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select distinct l.KeyId, isNull(ln.Value, lnd.Value) as Norwegian, isNull(le.value, led.value) as English");
            sql.AppendLine("from t_language l");
            sql.AppendLine("left join t_language ln on l.keyId = ln.keyId and ln.lang = 'no' and isNull(ln.customer, '') = @customer");
            sql.AppendLine("left join t_language le on l.keyId = le.keyId and le.lang = 'en' and isNull(le.customer, '') = @customer");
            sql.AppendLine("left join t_language lnd on l.keyId = lnd.keyId and lnd.lang = 'no' and isNull(lnd.customer, '') = ''");
            sql.AppendLine("left join t_language led on l.keyId = led.keyId and led.lang = 'en' and isNull(led.customer, '') = ''");
            sql.AppendLine("where  l.KeyId in @keyIds");

            return _connection.Query<ExternalTranslation>(sql.ToString(), new {customer, keyIds});
        }

        public IEnumerable<Log> GetLogs()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select top 500 * from t_history order by id desc");

            return _connection.Query<Log>(sql.ToString()).ToList();
        }

        private void UpdateCustomerKey(string keyId, string value, string customer, Language language, string updatedBy)
        {
            LogChange(keyId, language.ToString(), value, customer, updatedBy);

            var sql = new StringBuilder();

            var defaultValue = GetDefaultValue(keyId, language.ToString());

            if (KeyAlreadyExists(keyId, customer, language.ToString()))
            {
                if (value == defaultValue.Value || IsNullOrEmpty(value))
                {
                    sql.AppendLine("delete from t_language where keyId = @keyId and lang = @lang and customer = @customer");
                    _connection.Execute(sql.ToString(), new { keyId, lang = language.ToString(), customer });
                }
                else
                {
                    sql.AppendLine("update t_language set value = @value where keyId = @keyId and lang = @lang and customer = @customer");
                    _connection.Execute(sql.ToString(), new { value, keyId, lang = language.ToString(), customer });
                }
            }
            else if (value != defaultValue.Value && !IsNullOrEmpty(value))
            {
                sql.AppendLine("insert into t_language (id, keyId, lang, value, customer)");
                sql.AppendLine("values((select max(id) + 1 from t_language), @keyId, @lang, @value, @customer)");
                _connection.Execute(sql.ToString(), new { value, keyId, lang = language.ToString(), customer });
            }
        }

        private IEnumerable<Translation> GetDefaultValues()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from t_language where isNull(customer, '') = ''");

            return _connection.Query<Translation>(sql.ToString()).ToList();
        }

        private Translation GetDefaultValue(string key, string language)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from t_language where keyId = @key and lang = @language and isNull(customer, '') = ''");

            return _connection.Query<Translation>(sql.ToString(), new { key, language }).FirstOrDefault();
        }

        private IEnumerable<Translation> GetFilteredValues(string customer, string language)
        {
            StringBuilder sql = new StringBuilder();

            if (IsNullOrEmpty(customer))
            {
                // Get by language   
                sql.AppendLine("select * from");
                sql.AppendLine("(Select * from t_language where lang = @language and isNull(customer, '') = ''");
                sql.AppendLine("union select * from t_language where lang = @language");
                sql.AppendLine("and keyid not in (select keyid from t_language where lang = @language)) inni");
            }

            else if (IsNullOrEmpty(language))
            {
                // Get by customer
                sql.AppendLine("select l0.*, l2.value as DefaultValue from t_language l0");
                sql.AppendLine("left join t_language l2  on l0.keyid = l2.keyid and l0.lang = l2.lang  and not isNull(l0.customer, '') = '' and isNull(l2.customer, '') = ''");
                sql.AppendLine("where not l0.id in (");
                sql.AppendLine("select l.id from t_language l");
                sql.AppendLine("left join t_language l1 on l.keyid = l1.keyid and l.lang = l1.lang and l1.customer = @customer");
                sql.AppendLine("where not l.id = l1.id ) and isNull(l0.customer, @customer) = @customer");
            }

            else
            {
                // Get by both
                sql.AppendLine("select l0.*, l2.value as DefaultValue from t_language l0");
                sql.AppendLine("left join t_language l2  on l0.keyid = l2.keyid and l0.lang = l2.lang  and not isNull(l0.customer, '') = '' and isNull(l2.customer, '') = ''");
                sql.AppendLine("where not l0.id in (");
                sql.AppendLine("select l.id from t_language l");
                sql.AppendLine("left join t_language l1 on l.keyid = l1.keyid and l.lang = l1.lang and l1.customer = @customer");
                sql.AppendLine("where not l.id = l1.id )");
                sql.AppendLine("and isNull(l0.customer, @customer) = @customer and l0.lang = @language");
            }

            return _connection.Query<Translation>(sql.ToString(), new { customer, language }).ToList();
        }

        private bool KeyAlreadyExists(string keyId, string customer = "", string language = "")
        {
            var sql = new StringBuilder();
            sql.AppendLine("select count(*) from t_language where keyId = @keyId");

            if (!IsNullOrEmpty(customer))
            {
                sql.AppendLine("and customer = @customer");
            }

            if (!IsNullOrEmpty(language))
            {
                sql.AppendLine("and lang = @language");
            }

            var result = (int)_connection.ExecuteScalar(sql.ToString(), new { keyId, customer, language });

            return result > 0;
        }

        private string GetDataToExport()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select l1.keyId as KeyId, l1.value as Norwegian, l2.value as English,");
            sql.AppendLine("isNull(l1.customer, 'default') as Customer from t_language l1");
            sql.AppendLine("left join t_language l2 on l2.keyid = l1.keyid and l2.lang = 'en'");
            sql.AppendLine("and isNull(l2.customer, 'default') = isNull(l1.customer, 'default')");
            sql.AppendLine("where l1.lang = 'no'");
            sql.AppendLine("order by l1.customer, l1.keyid");

            var result = _connection.Query<ExternalTranslation>(sql.ToString()).ToList();

            return ConvertToCSV(result);
        } 

        private string ConvertToCSV(List<ExternalTranslation> list)
        {
            var sb = new StringBuilder();
            sb.Append("Key;Norwegian;English;Customer\r");

            foreach (var field in list)
            {
                sb.AppendFormat("{0};{1};{2};{3}{4}", field.KeyId, field.Norwegian, field.English, field.Customer, "\r");
            }
            sb.AppendLine();

            sb.Insert(0, '\uFEFF');

            return sb.ToString();
        }

        private void LogChange(string keyId, string lang, string value, string customer, string updatedBy)
        {
            if (IsNullOrEmpty(value))
                value = GetDefaultValue(keyId, lang).Value;

            StringBuilder sql = new StringBuilder();
            sql.AppendLine("insert into t_history (keyId, lang, oldValue, newValue, timestamp, customer, updatedBy)");
            sql.AppendLine("values(@keyId, @lang,");
            sql.AppendLine("(select case when len(l1.value) > 0 then l1.value else  l.value end from t_language l");
            sql.AppendLine("left join t_language l1 on l.keyId = l1.keyId and l1.lang = l.lang  and isNull(l1.customer, '') = @customer");
            sql.AppendLine("where l.keyId = @keyId and l.lang = @lang and l.customer is null),");
            sql.AppendLine("@newValue, getdate(), NULLIF(@customer, ''), @updatedBy)");

            _connection.Execute(sql.ToString(),
                new {keyId, lang, newValue = value, customer, updatedBy});
        }
    }
}