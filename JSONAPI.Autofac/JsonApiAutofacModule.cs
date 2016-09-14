using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using JSONAPI.ActionFilters;
using JSONAPI.Configuration;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;
using JSONAPI.Json;
using JSONAPI.QueryableResolvers;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.Autofac
{
    public class JsonApiAutofacModule : Module
    {
        private readonly IJsonApiConfiguration _jsonApiConfiguration;

        internal JsonApiAutofacModule(IJsonApiConfiguration jsonApiConfiguration)
        {
            _jsonApiConfiguration = jsonApiConfiguration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            // Register resource types
            var registry = new ResourceTypeRegistry();
            foreach (var resourceTypeConfiguration in _jsonApiConfiguration.ResourceTypeConfigurations)
            {
                var resourceTypeRegistration = resourceTypeConfiguration.BuildResourceTypeRegistration();
                registry.AddRegistration(resourceTypeRegistration);

                var configuration = resourceTypeConfiguration;
                builder.Register(c => configuration)
                    .Keyed<IResourceTypeConfiguration>(resourceTypeRegistration.Type)
                    .Keyed<IResourceTypeConfiguration>(resourceTypeRegistration.ResourceTypeName)
                    .SingleInstance();

                if (resourceTypeConfiguration.DocumentMaterializerType != null)
                    builder.RegisterType(resourceTypeConfiguration.DocumentMaterializerType);
                if (resourceTypeConfiguration.ResourceCollectionResolverType != null)
                    builder.RegisterType(resourceTypeConfiguration.ResourceCollectionResolverType);

                foreach (var relationship in resourceTypeRegistration.Relationships)
                {
                    IResourceTypeRelationshipConfiguration relationshipConfiguration;
                    if (resourceTypeConfiguration.RelationshipConfigurations
                        .TryGetValue(relationship.Property.Name, out relationshipConfiguration))
                    {
                        if (relationshipConfiguration.MaterializerType != null)
                        {
                            builder.RegisterType(relationshipConfiguration.MaterializerType);
                            continue;
                        }
                    }

                    // They didn't set an explicit materializer. See if they specified a factory for this resource type.
                    if (configuration.RelatedResourceMaterializerTypeFactory == null) continue;

                    var materializerType = configuration.RelatedResourceMaterializerTypeFactory(relationship);
                    builder.RegisterType(materializerType);
                }
            }

            builder.Register(c => registry).As<IResourceTypeRegistry>().SingleInstance();
            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                Func<string, IDocumentMaterializer> factory = resourceTypeName =>
                {
                    var configuration = context.ResolveKeyed<IResourceTypeConfiguration>(resourceTypeName);
                    var registration = registry.GetRegistrationForResourceTypeName(resourceTypeName);
                    var parameters = new Parameter[] { new TypedParameter(typeof (IResourceTypeRegistration), registration)};
                    if (configuration.ResourceCollectionResolverType != null)
                    {
                        var collectionResolver = context.Resolve(configuration.ResourceCollectionResolverType, parameters);
                        parameters = new Parameter[] { new TypedParameter(typeof(IResourceTypeRegistration), registration), new NamedParameter("collectionResolver", collectionResolver), };
                    }
                    if (configuration.DocumentMaterializerType != null)
                        return (IDocumentMaterializer)context.Resolve(configuration.DocumentMaterializerType, parameters);
                    return context.Resolve<IDocumentMaterializer>(parameters);
                };
                return factory;
            });
            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                Func<Type, IDocumentMaterializer> factory = clrType =>
                {
                    var configuration = context.ResolveKeyed<IResourceTypeConfiguration>(clrType);
                    var registration = registry.GetRegistrationForType(clrType);
                    var parameters = new List<Parameter> { new TypedParameter(typeof(IResourceTypeRegistration), registration)};
                    if (configuration.ResourceCollectionResolverType != null)
                    {
                        var collectionResolver = context.Resolve(configuration.ResourceCollectionResolverType, parameters);
                        parameters.Add(new NamedParameter("collectionResolver", collectionResolver));
                    }
                    if (configuration.DocumentMaterializerType != null)
                        return (IDocumentMaterializer)context.Resolve(configuration.DocumentMaterializerType, parameters);
                    return context.Resolve<IDocumentMaterializer>(parameters);
                };
                return factory;
            });
            builder.Register(c =>
            {
                var context = c.Resolve<IComponentContext>();
                Func<string, string, IRelatedResourceDocumentMaterializer> factory = (resourceTypeName, relationshipName) =>
                {
                    var configuration = context.ResolveKeyed<IResourceTypeConfiguration>(resourceTypeName);
                    var registration = registry.GetRegistrationForResourceTypeName(resourceTypeName);
                    var relationship = registration.GetFieldByName(relationshipName) as ResourceTypeRelationship;
                    if (relationship == null)
                        throw JsonApiException.CreateForNotFound(
                            string.Format("No relationship `{0}` exists for the resource type `{1}`.", relationshipName, resourceTypeName));
                    
                    var parameters = new List<Parameter>
                    {
                        new TypedParameter(typeof(IResourceTypeRegistration), registration),
                        new TypedParameter(typeof(ResourceTypeRelationship), relationship)
                    };
                    var relConfiguration = context.ResolveKeyed<IResourceTypeConfiguration>(relationshipName);
                    if (relConfiguration.ResourceCollectionResolverType != null)
                    {
                        var collectionResolver = context.Resolve(relConfiguration.ResourceCollectionResolverType, parameters);
                        parameters.Add(new NamedParameter("collectionResolver", collectionResolver));
                    }
                    // First, see if they have set an explicit materializer for this relationship
                    IResourceTypeRelationshipConfiguration relationshipConfiguration;
                    if (configuration.RelationshipConfigurations.TryGetValue(relationship.Property.Name,
                        out relationshipConfiguration) && relationshipConfiguration.MaterializerType != null)
                        return (IRelatedResourceDocumentMaterializer)context.Resolve(relationshipConfiguration.MaterializerType, parameters);

                    // They didn't set an explicit materializer. See if they specified a factory for this resource type.
                    if (configuration.RelatedResourceMaterializerTypeFactory != null)
                    {
                        var materializerType = configuration.RelatedResourceMaterializerTypeFactory(relationship);
                        return (IRelatedResourceDocumentMaterializer)context.Resolve(materializerType, parameters);
                    }

                    return context.Resolve<IRelatedResourceDocumentMaterializer>(parameters);
                };
                return factory;
            });

            builder.RegisterType<JsonApiHttpConfiguration>().SingleInstance();
            if (_jsonApiConfiguration.CustomBaseUrlService != null)
            {
                builder.Register(c => _jsonApiConfiguration.CustomBaseUrlService).As<IBaseUrlService>().SingleInstance();
            }
            else
            {
                builder.RegisterType<BaseUrlService>().As<IBaseUrlService>().SingleInstance();
            }
            builder.RegisterType<DocumentMaterializerLocator>().As<IDocumentMaterializerLocator>().InstancePerRequest();

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

            // Document building
            builder.Register(c => _jsonApiConfiguration.LinkConventions).As<ILinkConventions>().SingleInstance();
            builder.RegisterType<JsonApiFormatter>().SingleInstance();
            builder.RegisterType<RegistryDrivenResourceCollectionDocumentBuilder>().As<IResourceCollectionDocumentBuilder>().SingleInstance();
            builder.RegisterType<RegistryDrivenSingleResourceDocumentBuilder>().As<ISingleResourceDocumentBuilder>().SingleInstance();
            builder.RegisterType<FallbackDocumentBuilder>().As<IFallbackDocumentBuilder>().SingleInstance();
            builder.RegisterType<ErrorDocumentBuilder>().As<IErrorDocumentBuilder>().SingleInstance();
            builder.RegisterType<FallbackDocumentBuilderAttribute>().SingleInstance();
            builder.RegisterType<JsonApiExceptionFilterAttribute>().SingleInstance();
            builder.RegisterType<DefaultQueryableResourceCollectionDocumentBuilder>().As<IQueryableResourceCollectionDocumentBuilder>();

            // Misc
            builder.RegisterType<DefaultSortExpressionExtractor>().As<ISortExpressionExtractor>().SingleInstance();
            builder.RegisterType<DefaultIncludeExpressionExtractor>().As<IIncludeExpressionExtractor>().SingleInstance();
        }
    }
}
