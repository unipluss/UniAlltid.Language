using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Dapper;
using Newtonsoft.Json;

namespace UniAlltid.Language.API.Models
{
    public class LanguageRepository
    {
        private readonly IDbConnection _connection;

        public LanguageRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        internal IEnumerable<Translation> Retrieve(string customer, string language)
        {
            if (String.IsNullOrEmpty(customer) && String.IsNullOrEmpty(language))
                return GetDefaultValues();

            else
                return GetFilteredValues(customer, language);
        }

        internal Dictionary<string, string> RetrieveDictionary(string language, string customer)
        {
            var translations = GetFilteredValues(customer, language);

            var dict = translations.ToDictionary(translation => translation.KeyId, translation => translation.Value);

            return dict;
        }

        internal void Create(NewTranslation translation)
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

        internal void CreateOrUpdateSingle(NewSingleTranslation translation)
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

        internal void Update(Translation translation, string selectedCustomer)
        {
            StringBuilder sql = new StringBuilder();

            if (String.IsNullOrEmpty(translation.Customer) && String.IsNullOrEmpty(selectedCustomer))
            {
                sql.AppendFormat("update t_language set value = @value where id=@id");
                _connection.Execute(sql.ToString(), new { value = translation.Value, id = translation.Id });
            }
            else if (!String.IsNullOrEmpty(translation.Customer))
            {                          
                sql.AppendLine("select * from t_language where keyid = @keyid and lang = @lang and IsNull(customer, '') = ''");

                Translation defaultEntry =
                    _connection.Query<Translation>(sql.ToString(),
                        new { keyid = translation.KeyId, lang = translation.Lang }).First();

                if (translation.Value == defaultEntry.Value || String.IsNullOrEmpty(translation.Value))
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

        internal void Delete(int id)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("delete from t_language where id=@id");

            _connection.Execute(sql.ToString(), new {id});
        }

        internal IEnumerable<Customer> RetrieveCustomers()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from t_customer");

            return _connection.Query<Customer>(sql.ToString());
        } 

        private IEnumerable<Translation> GetDefaultValues()
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendLine("select * from t_language where isNull(customer, '') = ''");

            return _connection.Query<Translation>(sql.ToString()).ToList();
        }

         private IEnumerable<Translation> GetFilteredValues(string customer, string language)
        {
            StringBuilder sql = new StringBuilder();

            if (String.IsNullOrEmpty(customer))
            {
                // Get by language   
                sql.AppendLine("select * from");
                sql.AppendLine("(Select * from t_language where lang = @language and isNull(customer, '') = ''");
                sql.AppendLine("union select * from t_language where lang = @language");
                sql.AppendLine("and keyid not in (select keyid from t_language where lang = @language)) inni");
            }

            else if (String.IsNullOrEmpty(language))
            {
                // Get by customer
                sql.AppendLine("select l0.*, l2.value as DefaultValue from t_language l0");
                sql.AppendLine("left join t_language l2  on l0.keyid = l2.keyid and l0.lang = l2.lang  and not isNull(l0.customer, '') = '' and isNull(l2.customer, '') = ''");
                sql.AppendLine("where not l0.id in (");
                sql.AppendLine("select l.id from t_language l");
                sql.AppendLine("left join t_language l1 on l.keyid = l1.keyid and l.lang = l1.lang and l1.customer = @customer");
                sql.AppendLine("where not l.id = l1.id )");
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
                sql.AppendLine("and l0.lang = @language");
            }

            return _connection.Query<Translation>(sql.ToString(), new { customer, language }).ToList();
        }

        private bool KeyAlreadyExists(string keyId)
        {
            var result = (int)_connection.ExecuteScalar("select count(*) from t_language where keyId = @keyId", new { keyId });

            return result > 0;
        }

    }
}
