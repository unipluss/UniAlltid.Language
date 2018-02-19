
using System.Configuration;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using Mindscape.Raygun4Net.WebApi;
using Newtonsoft.Json.Serialization;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using UniAlltid.Language.API.Providers;
using AppSettingsReader = UniAlltid.Language.API.Code.AppSettingsReader;

namespace UniAlltid.Language.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            //config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

           //RaygunWebApiClient.Attach(config, () => new RaygunWebApiClient(ConfigurationManager.AppSettings["raygunApiKey"]));
           
            config.Services.Add(typeof(IExceptionLogger), new ExceptionLogProvider(GetLogger(), LogEventLevel.Error, "Uncaught exception in backend {RequestMethod} {RequestPath}"));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = 
                new CamelCasePropertyNamesContractResolver();

            config.EnableCors();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional}
            );
        }

        private static ILogger GetLogger()
        {
            var logSwitch = new LoggingLevelSwitch();
            var logConfig = new LoggerConfiguration();
            logConfig.Enrich.FromLogContext()
                .MinimumLevel.ControlledBy(logSwitch)

                .Enrich.WithProperty("servicetype", "unilanguage.api")

#if !DEBUG
                .WriteTo.Seq(AppSettingsReader.SeqUrl, apiKey: AppSettingsReader.SeqApiKey, controlLevelSwitch: logSwitch);

#endif
#if DEBUG
                // https://getseq.net
                .WriteTo.Seq(AppSettingsReader.SeqUrl, controlLevelSwitch: logSwitch);
#endif
            return logConfig.CreateLogger();
        }
    }
}
