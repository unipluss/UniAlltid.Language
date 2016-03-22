using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using UniAlltid.Language.API.Models;
using WebApi.OutputCache.V2;

namespace UniAlltid.Language.API.Controllers
{    
    [RoutePrefix("api/translation")]    
    public class TranslationController : BaseApiController
    {
        private const int serverCacheSeconds = (60 * 60 * 24);


        public TranslationController(IDbConnection connection, ILanguageRepository languageRepository) : base(connection, languageRepository)
        {
        }

        [Route("{language}")]
        [HttpGet]
        [CacheOutput(ClientTimeSpan = serverCacheSeconds, ServerTimeSpan = serverCacheSeconds, ExcludeQueryStringFromCacheKey = false)]
        public Dictionary<string, string> Get(string language, string customer = "")
        {
            return base._languageRepository.RetrieveDictionary(language, customer);
        }

        [HttpPost]
        public void CreateOrUpdateSingle([FromBody]NewSingleTranslation translation)
        {
            base._languageRepository.CreateOrUpdateSingle(translation);
            base.EmptyCache();
        }

        [Route("customer")]
        [HttpPost]
        public void UpdateCustomerKeys([FromBody]UpdateExternalTranslationRequest request)
        {
            base._languageRepository.UpdateCustomerKeys(request.Translations, request.Customer, request.UpdatedBy);
            base.EmptyCache();
        }
    }
}