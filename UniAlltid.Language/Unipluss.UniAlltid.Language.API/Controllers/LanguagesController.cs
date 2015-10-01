using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web.Http;
using System.Web.Http.Cors;
using Dapper;
using UniAlltid.Language.API.Code;
using UniAlltid.Language.API.Models;

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
        public void Post([FromBody]NewTranslation translation)
        {
            _repo.Create(translation);
        }


        [HttpPut]
        public void Put([FromBody]Translation translation, [FromUri] string selectedCustomer = "")
        {
            _repo.Update(translation, selectedCustomer);
        }

        public void Delete(int id)
        {
        }
    }
}
