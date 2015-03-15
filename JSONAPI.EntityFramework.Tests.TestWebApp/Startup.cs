using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
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
            var httpConfig = new HttpConfiguration();
            
            // Configure the model manager
            var pluralizationService = new PluralizationService();
            var modelManager = new ModelManager(pluralizationService)
                .RegisterResourceType(typeof (Comment))
                .RegisterResourceType(typeof (Post))
                .RegisterResourceType(typeof (Tag))
                .RegisterResourceType(typeof (User))
                .RegisterResourceType(typeof (UserGroup));

            // Configure JSON API
            new JsonApiConfiguration(modelManager)
                .UseEntityFramework()
                .Apply(httpConfig);


            // Web API routes
            httpConfig.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            return httpConfig;
        }
    }
}