using System;
using System.Collections.Generic;
using Autofac;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;
using JSONAPI.Json;

namespace JSONAPI.Autofac
{
    public class JsonApiAutofacModule : Module
    {
        private readonly INamingConventions _namingConventions;
        private readonly ILinkConventions _linkConventions;
        private readonly IEnumerable<Action<ResourceTypeRegistry>> _registrationActions;

        public JsonApiAutofacModule(INamingConventions namingConventions, ILinkConventions linkConventions,
            IEnumerable<Action<ResourceTypeRegistry>> registrationActions)
        {
            _namingConventions = namingConventions;
            _linkConventions = linkConventions;
            _registrationActions = registrationActions;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Registry
            builder.Register(c => _namingConventions).As<INamingConventions>().SingleInstance();
            builder.RegisterType<ResourceTypeRegistry>().AsSelf().SingleInstance();
            builder.Register(c =>
            {
                var registry = c.Resolve<ResourceTypeRegistry>();
                foreach (var registrationAction in _registrationActions)
                    registrationAction(registry);
                return registry;
            }).As<IResourceTypeRegistry>().SingleInstance();

            builder.RegisterType<JsonApiHttpConfiguration>();
            builder.RegisterType<BaseUrlService>().As<IBaseUrlService>();

            // Serialization
            builder.RegisterType<MetadataFormatter>().As<IMetadataFormatter>().SingleInstance();
            builder.RegisterType<LinkFormatter>().As<ILinkFormatter>().SingleInstance();
            builder.RegisterType<ResourceLinkageFormatter>().As<IResourceLinkageFormatter>().SingleInstance();
            builder.RegisterType<RelationshipObjectFormatter>().As<IRelationshipObjectFormatter>().SingleInstance();
            builder.RegisterType<ResourceObjectFormatter>().As<IResourceObjectFormatter>().SingleInstance();
            builder.RegisterType<SingleResourceDocumentFormatter>().As<ISingleResourceDocumentFormatter>().SingleInstance();
            builder.RegisterType<ResourceCollectionDocumentFormatter>().As<IResourceCollectionDocumentFormatter>().SingleInstance();
            builder.RegisterType<ErrorFormatter>().As<IErrorFormatter>().SingleInstance();
            builder.RegisterType<ErrorDocumentFormatter>().As<IErrorDocumentFormatter>().SingleInstance();

            // Queryable transforms
            builder.RegisterType<SynchronousEnumerationTransformer>().As<IQueryableEnumerationTransformer>().SingleInstance();
            builder.RegisterType<DefaultFilteringTransformer>().As<IQueryableFilteringTransformer>().SingleInstance();
            builder.RegisterType<DefaultSortingTransformer>().As<IQueryableSortingTransformer>().SingleInstance();
            builder.RegisterType<DefaultPaginationTransformer>().As<IQueryablePaginationTransformer>().SingleInstance();

            // document building
            var linkConventions = _linkConventions ?? new DefaultLinkConventions();
            builder.Register(c => linkConventions).As<ILinkConventions>().SingleInstance();
            builder.RegisterType<JsonApiFormatter>().SingleInstance();
            builder.RegisterType<RegistryDrivenResourceCollectionDocumentBuilder>().As<IResourceCollectionDocumentBuilder>().SingleInstance();
            builder.RegisterType<RegistryDrivenSingleResourceDocumentBuilder>().As<ISingleResourceDocumentBuilder>().SingleInstance();
            builder.RegisterType<FallbackDocumentBuilder>().As<IFallbackDocumentBuilder>().SingleInstance();
            builder.RegisterType<ErrorDocumentBuilder>().As<IErrorDocumentBuilder>().SingleInstance();
            builder.RegisterType<FallbackDocumentBuilderAttribute>().SingleInstance();
            builder.RegisterType<JsonApiExceptionFilterAttribute>().SingleInstance();
            builder.RegisterType<DefaultQueryableResourceCollectionDocumentBuilder>().As<IQueryableResourceCollectionDocumentBuilder>();

        }
    }
}
