using System;
using System.Data.Entity;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Autofac;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;
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

            var pluralizationService = new EntityFrameworkPluralizationService();
            var namingConventions = new DefaultNamingConventions(pluralizationService);

            var configuration = new JsonApiAutofacConfiguration(namingConventions);
            configuration.OnContainerBuilding(builder =>
            {
                builder.RegisterType<EntityFrameworkPayloadMaterializer>()
                    .WithParameter("apiBaseUrl", "https://www.example.com")
                    .As<IPayloadMaterializer>();
                builder.Register(c => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
                builder.Register(c => c.Resolve<IOwinContext>().Get<TestDbContext>(DbContextKey)).AsSelf().As<DbContext>();
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            });
            configuration.RegisterResourceType(typeof(City));
            configuration.RegisterResourceType(typeof(Comment));
            configuration.RegisterResourceType(typeof(Post));
            configuration.RegisterResourceType(typeof(Sample));
            configuration.RegisterResourceType(typeof(State));
            configuration.RegisterResourceType(typeof(Tag));
            configuration.RegisterResourceType(typeof(User));
            configuration.RegisterResourceType(typeof(UserGroup));

            var httpConfig = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            // Web API routes
            httpConfig.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            var container = configuration.Apply(httpConfig);

            //var appContainerBuilder = new ContainerBuilder();
            app.UseAutofacMiddleware(container);

            httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            app.UseWebApi(httpConfig);
            app.UseAutofacWebApi(httpConfig);
        }
    }
}