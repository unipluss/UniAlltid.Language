using System.Collections.Generic;
using System.Data;
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
        [InvalidateCacheOutput("Get", typeof(TranslationController))]
        public void Post([FromBody]NewTranslation translation)
        {
            _languageRepository.Create(translation);
            base.EmptyCache();
        }


        [HttpPut]
        [InvalidateCacheOutput("Get", typeof(TranslationController))]
        public void Put([FromBody]Translation translation, [FromUri] string selectedCustomer = "")
        {
            _languageRepository.Update(translation, selectedCustomer);
            base.EmptyCache();
        }

        [HttpDelete]
        [InvalidateCacheOutput("Get", typeof(TranslationController))]
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
    }
}
