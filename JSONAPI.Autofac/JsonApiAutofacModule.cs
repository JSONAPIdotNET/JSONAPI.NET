using System;
using System.Collections.Generic;
using Autofac;
using JSONAPI.ActionFilters;
using JSONAPI.Core;
using JSONAPI.Json;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;

namespace JSONAPI.Autofac
{
    public class JsonApiAutofacModule : Module
    {
        private readonly INamingConventions _namingConventions;
        private readonly ILinkConventions _linkConventions;
        private readonly IEnumerable<Type> _typesToRegister;

        public JsonApiAutofacModule(INamingConventions namingConventions, ILinkConventions linkConventions, IEnumerable<Type> typesToRegister)
        {
            _namingConventions = namingConventions;
            _linkConventions = linkConventions;
            _typesToRegister = typesToRegister;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Registry
            builder.Register(c => _namingConventions).As<INamingConventions>().SingleInstance();
            builder.RegisterType<ResourceTypeRegistry>().AsSelf().SingleInstance();
            builder.Register(c =>
            {
                var registry = c.Resolve<ResourceTypeRegistry>();
                foreach (var type in _typesToRegister)
                    registry.RegisterResourceType(type);
                return registry;
            }).As<IResourceTypeRegistry>().SingleInstance();

            builder.RegisterType<JsonApiHttpConfiguration>();

            // Serialization
            builder.RegisterType<MetadataSerializer>().As<IMetadataSerializer>().SingleInstance();
            builder.RegisterType<LinkSerializer>().As<ILinkSerializer>().SingleInstance();
            builder.RegisterType<ResourceLinkageSerializer>().As<IResourceLinkageSerializer>().SingleInstance();
            builder.RegisterType<RelationshipObjectSerializer>().As<IRelationshipObjectSerializer>().SingleInstance();
            builder.RegisterType<ResourceObjectSerializer>().As<IResourceObjectSerializer>().SingleInstance();
            builder.RegisterType<SingleResourcePayloadSerializer>().As<ISingleResourcePayloadSerializer>().SingleInstance();
            builder.RegisterType<ResourceCollectionPayloadSerializer>().As<IResourceCollectionPayloadSerializer>().SingleInstance();
            builder.RegisterType<ErrorSerializer>().As<IErrorSerializer>().SingleInstance();
            builder.RegisterType<ErrorPayloadSerializer>().As<IErrorPayloadSerializer>().SingleInstance();

            // Queryable transforms
            builder.RegisterType<SynchronousEnumerationTransformer>().As<IQueryableEnumerationTransformer>().SingleInstance();
            builder.RegisterType<DefaultFilteringTransformer>().As<IQueryableFilteringTransformer>().SingleInstance();
            builder.RegisterType<DefaultSortingTransformer>().As<IQueryableSortingTransformer>().SingleInstance();
            builder.RegisterType<DefaultPaginationTransformer>().As<IQueryablePaginationTransformer>().SingleInstance();

            // Payload building
            var linkConventions = _linkConventions ?? new DefaultLinkConventions();
            builder.Register(c => linkConventions).As<ILinkConventions>().SingleInstance();
            builder.RegisterType<JsonApiFormatter>().SingleInstance();
            builder.RegisterType<RegistryDrivenResourceCollectionPayloadBuilder>().As<IResourceCollectionPayloadBuilder>().SingleInstance();
            builder.RegisterType<RegistryDrivenSingleResourcePayloadBuilder>().As<ISingleResourcePayloadBuilder>().SingleInstance();
            builder.RegisterType<FallbackPayloadBuilder>().As<IFallbackPayloadBuilder>().SingleInstance();
            builder.RegisterType<ErrorPayloadBuilder>().As<IErrorPayloadBuilder>().SingleInstance();
            builder.RegisterType<FallbackPayloadBuilderAttribute>().SingleInstance();
            builder.RegisterType<JsonApiExceptionFilterAttribute>().SingleInstance();
            builder.RegisterType<DefaultQueryableResourceCollectionPayloadBuilder>().As<IQueryableResourceCollectionPayloadBuilder>();

        }
    }
}
