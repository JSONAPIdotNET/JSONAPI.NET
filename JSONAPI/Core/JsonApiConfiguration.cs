using System.Web.Http;
using JSONAPI.ActionFilters;
using JSONAPI.Http;
using JSONAPI.Json;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;

namespace JSONAPI.Core
{
    /// <summary>
    /// This is a convenience class for configuring JSONAPI.NET in the simplest way possible.
    /// </summary>
    public class JsonApiConfiguration
    {
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly ILinkConventions _linkConventions;
        private IQueryableEnumerationTransformer _queryableEnumerationTransformer;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiConfiguration(IResourceTypeRegistry resourceTypeRegistry)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
        }

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiConfiguration(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
        {
            _resourceTypeRegistry = resourceTypeRegistry;
            _linkConventions = linkConventions;
        }

        /// <summary>
        /// Allows overriding the queryable payload builder to use. This is useful for 
        /// </summary>
        /// <param name="queryableEnumerationTransformer"></param>
        public void UseQueryableEnumeration(IQueryableEnumerationTransformer queryableEnumerationTransformer)
        {
            _queryableEnumerationTransformer = queryableEnumerationTransformer;
        }

        /// <summary>
        /// Applies the running configuration to an HttpConfiguration instance
        /// </summary>
        /// <param name="httpConfig">The HttpConfiguration to apply this JsonApiConfiguration to</param>
        public void Apply(HttpConfiguration httpConfig)
        {
            var linkConventions = _linkConventions ?? new DefaultLinkConventions();

            // Serialization
            var metadataSerializer = new MetadataSerializer();
            var linkSerializer = new LinkSerializer(metadataSerializer);
            var resourceLinkageSerializer = new ResourceLinkageSerializer();
            var relationshipObjectSerializer = new RelationshipObjectSerializer(linkSerializer, resourceLinkageSerializer, metadataSerializer);
            var resourceObjectSerializer = new ResourceObjectSerializer(relationshipObjectSerializer, linkSerializer, metadataSerializer);
            var errorSerializer = new ErrorSerializer(linkSerializer, metadataSerializer);
            var singleResourcePayloadSerializer = new SingleResourcePayloadSerializer(resourceObjectSerializer, metadataSerializer);
            var resourceCollectionPayloadSerializer = new ResourceCollectionPayloadSerializer(resourceObjectSerializer, metadataSerializer);
            var errorPayloadSerializer = new ErrorPayloadSerializer(errorSerializer, metadataSerializer);

            // Queryable transforms
            var queryableEnumerationTransformer = _queryableEnumerationTransformer ?? new SynchronousEnumerationTransformer();
            var filteringTransformer = new DefaultFilteringTransformer(_resourceTypeRegistry);
            var sortingTransformer = new DefaultSortingTransformer(_resourceTypeRegistry);
            var paginationTransformer = new DefaultPaginationTransformer();

            // Builders
            var baseUrlService = new BaseUrlService();
            var singleResourcePayloadBuilder = new RegistryDrivenSingleResourcePayloadBuilder(_resourceTypeRegistry, linkConventions);
            var resourceCollectionPayloadBuilder = new RegistryDrivenResourceCollectionPayloadBuilder(_resourceTypeRegistry, linkConventions);
            var queryableResourcePayloadBuilder = new DefaultQueryableResourceCollectionPayloadBuilder(resourceCollectionPayloadBuilder,
                queryableEnumerationTransformer, filteringTransformer, sortingTransformer, paginationTransformer, baseUrlService);
            var errorPayloadBuilder = new ErrorPayloadBuilder();
            var fallbackPayloadBuilder = new FallbackPayloadBuilder(singleResourcePayloadBuilder,
                queryableResourcePayloadBuilder, resourceCollectionPayloadBuilder, baseUrlService);

            // Dependencies for JsonApiHttpConfiguration
            var formatter = new JsonApiFormatter(singleResourcePayloadSerializer, resourceCollectionPayloadSerializer, errorPayloadSerializer, errorPayloadBuilder);
            var fallbackPayloadBuilderAttribute = new FallbackPayloadBuilderAttribute(fallbackPayloadBuilder, errorPayloadBuilder);
            var exceptionFilterAttribute = new JsonApiExceptionFilterAttribute(errorPayloadBuilder, formatter);

            var jsonApiHttpConfiguration = new JsonApiHttpConfiguration(formatter, fallbackPayloadBuilderAttribute, exceptionFilterAttribute);
            jsonApiHttpConfiguration.Apply(httpConfig);
        }
    }
}
