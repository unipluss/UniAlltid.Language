using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using System.Web.Http.Cors;
using UniAlltid.Language.API.Code;
using UniAlltid.Language.API.Models;

namespace UniAlltid.Language.API.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/translation")]
    [TokenAccessFilter]
    public class TranslationController : ApiController
    {
        private readonly LanguageRepository _repo;

        public TranslationController(IDbConnection connection)
        {
            _repo = new LanguageRepository(connection);
        }

        [Route("{language}")]
        [HttpGet]
        public Dictionary<string, string> GetTranslations(string language, string customer = "")
        {
            return _repo.RetrieveDictionary(language, customer);
        }

        [HttpPost]
        public void CreateOrUpdateSingle(NewTranslation translation)
        {
            _repo.CreateOrUpdateSingle(translation);
        }
    }
}