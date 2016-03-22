using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using UniAlltid.Language.API.Code;
using UniAlltid.Language.API.Models;
using WebApi.OutputCache.V2;

namespace UniAlltid.Language.API.Controllers
{
    [RoutePrefix("api/languages")]
    public class LanguagesController : BaseApiController
    {
        public LanguagesController(IDbConnection connection, ILanguageRepository languageRepository) : base(connection, languageRepository)
        {
        }

        [HttpGet]
        public IEnumerable<Translation> Get(string customer="", string language="")
        {
            return _languageRepository.Retrieve(customer, language);
        }

        [HttpPost]
        public void Post([FromBody]NewTranslation translation)
        {
            _languageRepository.Create(translation);
            base.EmptyCache();
        }


        [HttpPut]
        public void Put([FromBody]Translation translation, [FromUri] string selectedCustomer = "")
        {
            _languageRepository.Update(translation, selectedCustomer);
            base.EmptyCache();
        }

        [HttpPut]
        [Route("key")]
        public void UpdateKey([FromBody]Translation translation)
        {
            _languageRepository.UpdateKey(translation.Id, translation.KeyId);
            base.EmptyCache();
        }


        [HttpDelete]
        public void Delete(int id)
        {
            _languageRepository.Delete(id);
            base.EmptyCache();
        }

        [Route("customer")]
        [HttpGet]
        [CacheOutput(ClientTimeSpan = 5 * 60, ServerTimeSpan = 5*60, ExcludeQueryStringFromCacheKey = false)]
        public IEnumerable<Customer> GetCustomer()
        {
            return _languageRepository.RetrieveCustomers();
        }

        [Route("customer")]
        [HttpPost]
        [InvalidateCacheOutput("GetCustomer")]
        public void CreateCustomer(Customer customer)
        {
            _languageRepository.CreateCustomer(customer);
        }

        [Route("export")]
        [HttpGet]
        public HttpResponseMessage GetCsv()
        {
            return _languageRepository.ExportCSV();
        }

        [Route("logs")]
        [HttpGet]
        public IEnumerable<Log> GetLogs()
        {
            return _languageRepository.GetLogs();
        }
    }
}
