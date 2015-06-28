using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Autofac;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Http;
using JSONAPI.Http;
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
            var pluralizationService = new PluralizationService();
            pluralizationService.AddMapping("todo", "todos");
            var namingConventions = new DefaultNamingConventions(pluralizationService);

            var httpConfig = new HttpConfiguration();
            var configuration = new JsonApiAutofacConfiguration(namingConventions);
            configuration.RegisterResourceType(typeof(Todo));
            configuration.OnContainerBuilding(builder =>
            {
                builder.RegisterType<EntityFrameworkPayloadMaterializer>()
                    .WithParameter("apiBaseUrl", "https://www.example.com")
                    .As<IPayloadMaterializer>();
            });
            var container = configuration.Apply(httpConfig);

            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);

            // Web API routes
            httpConfig.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            return httpConfig;
        }
    }
}