using System.Web.Http;
using Autofac;
using JSONAPI.Autofac;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Http;
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

            var pluralizationService = new PluralizationService();
            pluralizationService.AddMapping("todo", "todos");
            var namingConventions = new DefaultNamingConventions(pluralizationService);

            var configuration = new JsonApiAutofacConfiguration(namingConventions);
            configuration.RegisterResourceType(typeof(Todo));
            var module = configuration.GetAutofacModule();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(module);
            containerBuilder.RegisterGeneric(typeof(EntityFrameworkPayloadMaterializer<>))
                .WithParameter("apiBaseUrl", "https://www.example.com")
                .AsImplementedInterfaces();
            var container = containerBuilder.Build();
            httpConfig.UseJsonApiWithAutofac(container);

            // Web API routes
            httpConfig.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            return httpConfig;
        }
    }
}