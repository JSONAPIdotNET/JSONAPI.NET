using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.Autofac;
using JSONAPI.Autofac.EntityFramework;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Http;
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

            var pluralizationService = new EntityFrameworkPluralizationService();
            var namingConventions = new DefaultNamingConventions(pluralizationService);

            var configuration = new JsonApiAutofacConfiguration(namingConventions);
            configuration.RegisterResourceType(typeof(City));
            configuration.RegisterResourceType(typeof(Comment));
            configuration.RegisterResourceType(typeof(Language));
            configuration.RegisterResourceType(typeof(LanguageUserLink),
                sortByIdFactory: LanguageUserLinkSortByIdFactory,
                filterByIdFactory: LanguageUserLinkFilterByIdFactory);
            configuration.RegisterResourceType(typeof(Post));
            configuration.RegisterResourceType(typeof(Sample));
            configuration.RegisterResourceType(typeof(State));
            configuration.RegisterResourceType(typeof(Tag));
            configuration.RegisterResourceType(typeof(User));
            configuration.RegisterResourceType(typeof(UserGroup));
            var module = configuration.GetAutofacModule();
            var efModule = configuration.GetEntityFrameworkAutofacModule();

            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterModule(module);
            containerBuilder.RegisterModule(efModule);
            containerBuilder.Register(c => HttpContext.Current.GetOwinContext()).As<IOwinContext>();
            containerBuilder.Register(c => c.Resolve<IOwinContext>().Get<TestDbContext>(DbContextKey)).AsSelf().As<DbContext>();
            containerBuilder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            containerBuilder.RegisterType<CustomEntityFrameworkResourceObjectMaterializer>()
                .As<IEntityFrameworkResourceObjectMaterializer>();
            var container = containerBuilder.Build();

            var httpConfig = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };
            httpConfig.UseJsonApiWithAutofac(container);

            // Web API routes
            httpConfig.Routes.MapHttpRoute("ResourceCollection", "{controller}");
            httpConfig.Routes.MapHttpRoute("Resource", "{controller}/{id}");
            httpConfig.Routes.MapHttpRoute("RelatedResource", "{controller}/{id}/{relationshipName}");

            app.UseAutofacMiddleware(container);

            app.UseWebApi(httpConfig);
            app.UseAutofacWebApi(httpConfig);
        }

        private BinaryExpression LanguageUserLinkFilterByIdFactory(ParameterExpression param, string id)
        {
            var split = id.Split('_');
            var languageId = Expression.Constant(split[0]);
            var userId = Expression.Constant(split[1]);

            var languageIdPropertyExpr = Expression.Property(param, "LanguageId");
            var languageIdPropertyEqualsExpr = Expression.Equal(languageIdPropertyExpr, languageId);

            var userIdPropertyExpr = Expression.Property(param, "UserId");
            var userIdPropertyEqualsExpr = Expression.Equal(userIdPropertyExpr, userId);

            return Expression.AndAlso(languageIdPropertyEqualsExpr, userIdPropertyEqualsExpr);
        }

        private Expression LanguageUserLinkSortByIdFactory(ParameterExpression param)
        {
            var concatMethod = typeof(string).GetMethod("Concat", new[] { typeof(object), typeof(object) });

            var languageIdExpr = Expression.Property(param, "LanguageId");
            var userIdExpr = Expression.Property(param, "UserId");
            return Expression.Call(concatMethod, languageIdExpr, userIdExpr);
        }
    }
}