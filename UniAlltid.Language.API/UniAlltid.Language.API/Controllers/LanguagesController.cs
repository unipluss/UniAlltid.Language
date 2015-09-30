using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using Dapper;
using UniAlltid.Language.API.Models;

namespace UniAlltid.Language.API.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/languages")]
    public class LanguagesController : ApiController
    {
        private readonly IDbConnection _connection;
        private readonly LanguageRepository _repo;
       

        public LanguagesController(IDbConnection connection)
        {
            _connection = connection;
            _repo = new LanguageRepository(_connection);
        }

        // GET: api/languages
        [HttpGet]
        public IEnumerable<Translation> Get(string customer="", string language="")
        {
            return _repo.Retrieve(customer, language);
        }

        // GET: api/languages/language?customer=optional
        [Route("{language}")]
        [HttpGet]
        public Dictionary<string, string> GetTranslations(string language, string customer = "")
        {
            var translations = GetFilteredValues(customer, language);

            var dict = translations.ToDictionary(translation => translation.KeyId, translation => translation.Value);

            return dict;
        }

        // POST: api/languages
        [HttpPost]
        public void Post([FromBody]NewTranslation translation)
        {
            //TODO: Check if key exists

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

        // PUT: api/languages/5
        [HttpPut]
        public void Put([FromBody]Translation translation, [FromUri] string selectedCustomer = "")
        {
            StringBuilder sql = new StringBuilder();


            // if customer = "": update t_language set value = value where id = id
            if (String.IsNullOrEmpty(translation.Customer) && String.IsNullOrEmpty(selectedCustomer))
            {
                sql.AppendFormat("update t_language set value = @value where id=@id");
                _connection.Execute(sql.ToString(), new {value = translation.Value, id = translation.Id});
            }
            else if (!String.IsNullOrEmpty(translation.Customer))
            {
                // Get object with same keyid and language                            
                sql.AppendLine("select * from t_language where keyid = @keyid and lang = @lang and IsNull(customer, '') = ''");

                Translation defaultEntry =
                    _connection.Query<Translation>(sql.ToString(),
                        new {keyid = translation.KeyId, lang = translation.Lang}).First();

                // Check if value = value. If yes, delete custom value entry in db (set to default value)
                if (translation.Value == defaultEntry.Value)
                {
                    // delete lanaguage from db
                    sql = new StringBuilder();
                    sql.AppendLine("delete from t_language where id=@id");
                    _connection.Execute(sql.ToString(), new {id = translation.Id});
                }
                else
                {
                    sql = new StringBuilder();
                    sql.AppendFormat("update t_language set value = @value where id=@id");
                    _connection.Execute(sql.ToString(), new {value = translation.Value, id = translation.Id});
                }
            }
            else
            {
                // insert new custom
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

        // DELETE: api/languages/5
        public void Delete(int id)
        {
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
    }
}
