using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using System.Web.Http.Cors;
using UniAlltid.Language.API.Code;
using UniAlltid.Language.API.Models;
using WebApi.OutputCache.V2;

namespace UniAlltid.Language.API.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/languages")]
    [TokenAccessFilter]
    public class LanguagesController : ApiController
    {
        private readonly LanguageRepository _repo;
   

        public LanguagesController(IDbConnection connection)
        {
            _repo = new LanguageRepository(connection);
        }

        [HttpGet]
        
        public IEnumerable<Translation> Get(string customer="", string language="")
        {
            return _repo.Retrieve(customer, language);
        }

        [HttpPost]
        [InvalidateCacheOutput("Get", typeof(TranslationController))]
        public void Post([FromBody]NewTranslation translation)
        {
            _repo.Create(translation);
        }


        [HttpPut]
        [InvalidateCacheOutput("Get", typeof(TranslationController))]
        public void Put([FromBody]Translation translation, [FromUri] string selectedCustomer = "")
        {
            _repo.Update(translation, selectedCustomer);
            //var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            //cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((TranslationController t) => t.Get(null, null)));
        }

        [HttpDelete]
        [InvalidateCacheOutput("Get", typeof(TranslationController))]
        public void Delete(int id)
        {
            _repo.Delete(id);
        }

        [Route("customer")]
        [HttpGet]
        [CacheOutput(ClientTimeSpan = 5 * 60, ServerTimeSpan = 5*60, ExcludeQueryStringFromCacheKey = false)]
        public IEnumerable<Customer> GetCustomer()
        {
            return _repo.RetrieveCustomers();
        }

        [Route("customer")]
        [HttpPost]
        [InvalidateCacheOutput("GetCustomer")]
        public void CreateCustomer(Customer customer)
        {
            _repo.CreateCustomer(customer);
        }
    }
}
