using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Owin;
using Owin;
using NWebsec.Owin;
using UniAlltid.Language.API.Controllers;

[assembly: OwinStartup(typeof(UniAlltid.Language.API.Startup))]

namespace UniAlltid.Language.API
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            config.MapHttpAttributeRoutes();

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

            IContainer container = builder.Build();
            config.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            GlobalConfiguration.Configuration.DependencyResolver = config.DependencyResolver;

            app.UseAutofacMiddleware(container);
            app.UseAutofacWebApi(config);
        }
    }
}
