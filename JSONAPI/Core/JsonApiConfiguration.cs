using System.Web.Http;
using JSONAPI.ActionFilters;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;
using JSONAPI.Json;

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
        /// Allows overriding the queryable document builder to use. This is useful for 
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
            var metadataFormatter = new MetadataFormatter();
            var linkFormatter = new LinkFormatter(metadataFormatter);
            var resourceLinkageFormatter = new ResourceLinkageFormatter();
            var relationshipObjectFormatter = new RelationshipObjectFormatter(linkFormatter, resourceLinkageFormatter, metadataFormatter);
            var resourceObjectFormatter = new ResourceObjectFormatter(relationshipObjectFormatter, linkFormatter, metadataFormatter);
            var errorFormatter = new ErrorFormatter(linkFormatter, metadataFormatter);
            var singleResourceDocumentFormatter = new SingleResourceDocumentFormatter(resourceObjectFormatter, metadataFormatter);
            var resourceCollectionDocumentFormatter = new ResourceCollectionDocumentFormatter(resourceObjectFormatter, metadataFormatter);
            var errorDocumentFormatter = new ErrorDocumentFormatter(errorFormatter, metadataFormatter);

            // Queryable transforms
            var queryableEnumerationTransformer = _queryableEnumerationTransformer ?? new SynchronousEnumerationTransformer();
            var filteringTransformer = new DefaultFilteringTransformer(_resourceTypeRegistry);
            var sortingTransformer = new DefaultSortingTransformer(_resourceTypeRegistry);
            var paginationTransformer = new DefaultPaginationTransformer();

            // Builders
            var baseUrlService = new BaseUrlService();
            var singleResourceDocumentBuilder = new RegistryDrivenSingleResourceDocumentBuilder(_resourceTypeRegistry, linkConventions);
            var resourceCollectionDocumentBuilder = new RegistryDrivenResourceCollectionDocumentBuilder(_resourceTypeRegistry, linkConventions);
            var queryableResourceCollectionDocumentBuilder = new DefaultQueryableResourceCollectionDocumentBuilder(resourceCollectionDocumentBuilder,
                queryableEnumerationTransformer, filteringTransformer, sortingTransformer, paginationTransformer, baseUrlService);
            var errorDocumentBuilder = new ErrorDocumentBuilder();
            var fallbackDocumentBuilder = new FallbackDocumentBuilder(singleResourceDocumentBuilder,
                queryableResourceCollectionDocumentBuilder, resourceCollectionDocumentBuilder, baseUrlService);

            // Dependencies for JsonApiHttpConfiguration
            var formatter = new JsonApiFormatter(singleResourceDocumentFormatter, resourceCollectionDocumentFormatter, errorDocumentFormatter, errorDocumentBuilder);
            var fallbackDocumentBuilderAttribute = new FallbackDocumentBuilderAttribute(fallbackDocumentBuilder, errorDocumentBuilder);
            var exceptionFilterAttribute = new JsonApiExceptionFilterAttribute(errorDocumentBuilder, formatter);

            var jsonApiHttpConfiguration = new JsonApiHttpConfiguration(formatter, fallbackDocumentBuilderAttribute, exceptionFilterAttribute);
            jsonApiHttpConfiguration.Apply(httpConfig);
        }
    }
}
