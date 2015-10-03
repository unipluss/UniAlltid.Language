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
    [RoutePrefix("api/translation")]
    [TokenAccessFilter]
    public class TranslationController : ApiController
    {
        private readonly LanguageRepository _repo;
        private const int serverCacheSeconds = (60 * 60 * 2);

        public TranslationController(IDbConnection connection)
        {
            _repo = new LanguageRepository(connection);
        }

        [Route("{language}")]
        [HttpGet]
        [CacheOutput(ClientTimeSpan = serverCacheSeconds, ServerTimeSpan = serverCacheSeconds, ExcludeQueryStringFromCacheKey = false)]
        public Dictionary<string, string> Get(string language, string customer = "")
        {
            return _repo.RetrieveDictionary(language, customer);
        }

        [HttpPost]
        [InvalidateCacheOutput("Get")]
        public void CreateOrUpdateSingle(NewSingleTranslation translation)
        {
            _repo.CreateOrUpdateSingle(translation);
        }
    }
}