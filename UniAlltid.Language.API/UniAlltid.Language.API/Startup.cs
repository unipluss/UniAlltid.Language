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

            app.UseWebApi(config);

            ConfigureAuth(app);
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
