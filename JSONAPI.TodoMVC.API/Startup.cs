using System.Web.Http;
using System.Web.Http.Dispatcher;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.EntityFramework.ActionFilters;
using JSONAPI.Http;
using JSONAPI.Json;
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

            var config = new HttpConfiguration();

            var modelManager = new ModelManager(pluralizationService);
            modelManager.RegisterResourceType(typeof(Todo));

            var formatter = new JsonApiFormatter(modelManager);
            config.Formatters.Clear();
            config.Formatters.Add(formatter);

            // Global filters
            config.Filters.Add(new EnumerateQueryableAsyncAttribute());
            config.Filters.Add(new EnableSortingAttribute(modelManager));
            config.Filters.Add(new EnableFilteringAttribute(modelManager));

            // Override controller selector
            config.Services.Replace(typeof(IHttpControllerSelector), new PascalizedControllerSelector(config));

            // Web API routes
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            return config;
        }
    }
}