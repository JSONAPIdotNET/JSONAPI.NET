using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.EntityFramework.ActionFilters;
using JSONAPI.Json;
using JSONAPI.TodoMVC.API.Controllers;
using JSONAPI.TodoMVC.API.Models;
using Microsoft.Owin;
using Owin;

namespace JSONAPI.TodoMVC.API
{
    public class Startup
    {
        private const string DbContextKey = "TodoMVC.DbContext";

        private readonly Func<IOwinContext, TodoMvcContext> _dbContextFactory;

        public Startup()
            : this(context => new TodoMvcContext())
        {
            
        }

        public Startup(Func<IOwinContext, TodoMvcContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Configuration(IAppBuilder app)
        {
            // Setup db context for use in DI
            app.Use(async (context, next) =>
            {
                TodoMvcContext dbContext = _dbContextFactory(context);
                context.Set(DbContextKey, dbContext);

                await next();

                dbContext.Dispose();
            });

            var appContainerBuilder = new ContainerBuilder();
            appContainerBuilder.Register(ctx => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
            appContainerBuilder.Register(c =>
            {
                return c.Resolve<IOwinContext>().Get<TodoMvcContext>(DbContextKey);
            }).As<TodoMvcContext>();
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
            var pluralizationService = new PluralizationService(new Dictionary<string, string>
            {
                { "todo", "todos" }
            });

            var config = new HttpConfiguration();

            var modelManager = new ModelManager(pluralizationService);

            var formatter = new JsonApiFormatter(modelManager);
            config.Formatters.Clear();
            config.Formatters.Add(formatter);

            // Global filters
            config.Filters.Add(new EnumerateQueryableAsyncAttribute());
            config.Filters.Add(new EnableFilteringAttribute(modelManager));

            // Web API routes
            config.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            return config;
        }
    }
}