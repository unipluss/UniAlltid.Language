using System.Collections.Generic;
using System.Data;
using System.Web.Http;
using System.Web.Http.Cors;

namespace UniAlltid.Language.API.Controllers
{
    [EnableCors("*", "*", "*")]
    [RoutePrefix("api/translation")]
    public class TranslationController : ApiController
    {
        private readonly IDbConnection _connection;

        public TranslationController(IDbConnection connection)
        {
            _connection = connection;
        }

        [Route("{language}")]
        [HttpGet]
        public Dictionary<string, string> GetTranslations(string language, string customer = "")
        {
            return new Dictionary<string, string>()
            {
                { language, "test" }
            };
        }
    }
}
