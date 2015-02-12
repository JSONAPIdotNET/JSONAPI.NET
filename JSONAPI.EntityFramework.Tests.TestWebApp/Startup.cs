using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.EntityFramework.ActionFilters;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;
using JSONAPI.Json;
using Microsoft.Owin;
using Owin;

namespace JSONAPI.EntityFramework.Tests.TestWebApp
{
    public class Startup
    {
        private const string DbContextKey = "TestWebApp.DbContext";

        private readonly Func<IOwinContext, TestDbContext> _dbContextFactory;

        public Startup()
            : this(context => new TestDbContext())
        {

        }

        public Startup(Func<IOwinContext, TestDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Configuration(IAppBuilder app)
        {
            // Setup db context for use in DI
            app.Use(async (context, next) =>
            {
                TestDbContext dbContext = _dbContextFactory(context);
                context.Set(DbContextKey, dbContext);

                await next();

                dbContext.Dispose();
            });

            var appContainerBuilder = new ContainerBuilder();
            appContainerBuilder.Register(ctx => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
            appContainerBuilder.Register(c => c.Resolve<IOwinContext>().Get<TestDbContext>(DbContextKey)).As<TestDbContext>();
            appContainerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            var appContainer = appContainerBuilder.Build();
            app.UseAutofacMiddleware(appContainer);

            var httpConfig = GetWebApiConfiguration();
            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(appContainer);
            app.UseWebApi(httpConfig);
            app.UseAutofacWebApi(httpConfig);
        }

        private static HttpConfiguration GetWebApiConfiguration()
        {
            var config = new HttpConfiguration();

            var pluralizationService = new PluralizationService();
            var modelManager = new ModelManager(pluralizationService);

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