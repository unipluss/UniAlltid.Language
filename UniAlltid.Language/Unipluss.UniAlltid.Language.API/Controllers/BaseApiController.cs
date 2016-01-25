using System.Data;
using System.Web.Http;
using System.Web.Http.Cors;
using UniAlltid.Language.API.Code;
using UniAlltid.Language.API.Models;
using WebApi.OutputCache.V2;

namespace UniAlltid.Language.API.Controllers
{
    [EnableCors("*", "*", "*")]
    [TokenAccessFilter]
    public class BaseApiController : ApiController
    {
        private readonly IDbConnection _connection;
        protected readonly ILanguageRepository _languageRepository;

        public BaseApiController(IDbConnection connection, ILanguageRepository languageRepository)
        {
            _connection = connection;
            _languageRepository = languageRepository;
        }

        protected void EmptyCache()
        {
            var cache = Configuration.CacheOutputConfiguration().GetCacheOutputProvider(Request);
            cache.RemoveStartsWith(Configuration.CacheOutputConfiguration().MakeBaseCachekey((TranslationController t) => t.Get(null, null)));
        }

    }
}