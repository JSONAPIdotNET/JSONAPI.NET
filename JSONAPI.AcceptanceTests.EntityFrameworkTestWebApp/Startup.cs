﻿using System;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.DocumentMaterializers;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.Autofac;
using JSONAPI.Autofac.EntityFramework;
using JSONAPI.Configuration;
using JSONAPI.EntityFramework;
using JSONAPI.EntityFramework.Configuration;
using Owin;
using System.Collections.Generic;
using JSONAPI.Documents.Builders;
using JSONAPI.EntityFramework.Documents.Builders;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp
{
    public class Startup
    {
        private readonly Func<TestDbContext> _dbContextFactory;

        public Startup()
            : this(() => new TestDbContext())
        {

        }

        public Startup(Func<TestDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public void Configuration(IAppBuilder app)
        {
            /* these steps are divided in multiple methods to support better acceptance tests
             * in production all the steps can be made inside the Configuration method.
            */
            var configuration = BuildConfiguration();

            var configurator = BuildAutofacConfigurator(app);

            var httpConfig = BuildHttpConfiguration();

            MergeAndSetupConfiguration(app, configurator, httpConfig, configuration);
        }

        internal void MergeAndSetupConfiguration(IAppBuilder app, JsonApiHttpAutofacConfigurator configurator,
            HttpConfiguration httpConfig, JsonApiConfiguration configuration)
        {
            configurator.Apply(httpConfig, configuration);
            app.UseWebApi(httpConfig);
            app.UseAutofacWebApi(httpConfig);
        }

        /// <summary>
        /// Build up the <see cref="JsonApiConfiguration"/> which registers all the model types and their mappings.
        /// </summary>
        /// <returns></returns>
        internal JsonApiConfiguration BuildConfiguration()
        {
            var configuration = new JsonApiConfiguration(
                new Core.PluralizationService(
                    new Dictionary<string, string> { { "Child", "Children" } }));
            configuration.RegisterEntityFrameworkResourceType<Building>();
            configuration.RegisterEntityFrameworkResourceType<City>();
            configuration.RegisterEntityFrameworkResourceType<Comment>();
            configuration.RegisterEntityFrameworkResourceType<Company>();
            configuration.RegisterEntityFrameworkResourceType<Language>();
            configuration.RegisterEntityFrameworkResourceType<LanguageUserLink>(c =>
            {
                c.OverrideDefaultFilterById(LanguageUserLinkFilterByIdFactory);
                c.OverrideDefaultSortById(LanguageUserLinkSortByIdFactory);
            });
            configuration.RegisterResourceType<Post>();
            configuration.RegisterResourceType<PostID>();
            configuration.RegisterResourceType<PostLongId>();
            configuration.RegisterEntityFrameworkResourceType<State>();
            configuration.RegisterEntityFrameworkResourceType<Tag>();
            configuration.RegisterEntityFrameworkResourceType<User>();
            configuration.RegisterEntityFrameworkResourceType<UserGroup>();
            configuration.RegisterResourceType<Sample>(); // Example of a resource type not controlled by EF
            configuration.RegisterMappedType<Starship, StarshipDto, StarshipDocumentMaterializer>(c =>
            {
                c.ConfigureRelationship(s => s.Officers,
                    rc => rc.UseMaterializer<StarshipOfficersRelatedResourceMaterializer>());
                c.ConfigureRelationship(s => s.ShipCounselor,
                    rc => rc.UseMaterializer<StarshipShipCounselorRelatedResourceMaterializer>());
            }); // Example of a resource that is mapped from a DB entity
            configuration.RegisterResourceType<StarshipOfficerDto>();
            configuration.RegisterEntityFrameworkResourceType<Master>();
            configuration.RegisterEntityFrameworkResourceType<Child>();
            return configuration;
        }

        /// <summary>
        /// Build up the <see cref="JsonApiHttpAutofacConfigurator"/> which registers <see cref="DbContext"/>, modules and materializer
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        internal JsonApiHttpAutofacConfigurator BuildAutofacConfigurator(IAppBuilder app)
        {
            var configurator = new JsonApiHttpAutofacConfigurator();
            configurator.OnApplicationLifetimeScopeCreating(builder =>
            {
                builder.Register(c => _dbContextFactory())
                    .AsSelf()
                    .As<DbContext>()
                    .InstancePerRequest();
                builder.RegisterModule<JsonApiAutofacEntityFrameworkModule>();
                builder.RegisterType<CustomEntityFrameworkResourceObjectMaterializer>()
                    .As<IEntityFrameworkResourceObjectMaterializer>();
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                builder.RegisterType<EntityFrameworkQueryableResourceCollectionDocumentBuilder>().As<IQueryableResourceCollectionDocumentBuilder>();

            });
            configurator.OnApplicationLifetimeScopeBegun(applicationLifetimeScope =>
            {
                // TODO: is this a candidate for spinning into a JSONAPI.Autofac.WebApi.Owin package? Yuck
                app.UseAutofacMiddleware(applicationLifetimeScope);
            });
            return configurator;
        }

        /// <summary>
        /// Build up the <see cref="HttpConfiguration"/> with additional routes
        /// </summary>
        /// <returns></returns>
        internal HttpConfiguration BuildHttpConfiguration()
        {
            var httpConfig = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always
            };

            // Additional Web API routes
            httpConfig.Routes.MapHttpRoute("Samples", "samples", new { Controller = "Samples" });
            httpConfig.Routes.MapHttpRoute("Search", "search", new { Controller = "Search" });
            httpConfig.Routes.MapHttpRoute("Trees", "trees", new { Controller = "Trees" });
            return httpConfig;
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