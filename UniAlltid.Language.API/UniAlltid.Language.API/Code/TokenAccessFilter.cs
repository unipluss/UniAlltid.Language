using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace UniAlltid.Language.API.Code
{
    public class TokenAccessFilter : FilterAttribute, System.Web.Http.Filters.IActionFilter
    {
        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            if (actionContext.Request.Headers.Contains("X-AccessToken"))
            {
                var tokenvalue = actionContext.Request.Headers.GetValues("X-AccessToken").FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(tokenvalue) && tokenvalue.Equals(""))
                {
                    return continuation();
                }
            }


            return Task.Factory.StartNew(() =>
            {
                var retur = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content =new StringContent("This resource cannot be used without accesstoken")
                };

                return retur;
            });

        }
    }
}