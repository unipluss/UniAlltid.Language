using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using NWebsec.Owin;
using Owin;
using UniAlltid.Language.API;
using UniAlltid.Language.API.Code.Compression;
using UniAlltid.Language.API.Models;
using AppSettingsReader = UniAlltid.Language.API.Code.AppSettingsReader;

[assembly: OwinStartup(typeof(Startup))]

namespace UniAlltid.Language.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();
            config.MessageHandlers.Insert(0, new CompressionHandler());

            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["UniConnection"].ConnectionString);
            con.Open();

            SetupAutofac(app, config);

            // app.UseWebApi(config);    FY FY!

            ConfigureAuth(app);

            app.UseHsts(options => options.MaxAge(days: 365));
            app.UseXfo(options => options.SameOrigin());
            app.UseXContentTypeOptions();
            app.UseXDownloadOptions();
            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            app.UseCsp(options => options
                .DefaultSources(s => s.Self())
                );

        }

        private void SetupAutofac(IAppBuilder app, HttpConfiguration config)
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());

            builder.Register(c =>
            {
                SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["UniConnection"].ConnectionString);
                con.Open();
                return con;
            }).As<IDbConnection>().InstancePerRequest();

            builder.Register(c => new UpdateOptions()
            {
                UpdateQueryId4 = AppSettingsReader.UpdateQueryId4
            }).As<UpdateOptions>().SingleInstance();

            builder.RegisterType<LanguageRepository>().As<ILanguageRepository>().InstancePerRequest();

            IContainer container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = config.DependencyResolver;

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
        }
    }
}
