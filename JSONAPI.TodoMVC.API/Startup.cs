using System.Data.Entity;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Autofac;
using JSONAPI.Autofac.EntityFramework;
using JSONAPI.Configuration;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Configuration;
using JSONAPI.TodoMVC.API.Models;
using Owin;

namespace JSONAPI.TodoMVC.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfig = GetWebApiConfiguration();
            app.UseWebApi(httpConfig);
        }

        private static HttpConfiguration GetWebApiConfiguration()
        {
            var httpConfig = new HttpConfiguration();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            containerBuilder.RegisterType<TodoMvcContext>().As<DbContext>().InstancePerRequest();
            var container = containerBuilder.Build();

            var configuration = new JsonApiConfiguration();
            configuration.RegisterEntityFrameworkResourceType<Todo>(c => c.OverrideDefaultResourceTypeName("todos"));
            configuration.SetupHttpConfigurationUsingAutofac(httpConfig, container);

            return httpConfig;
        }
    }
}