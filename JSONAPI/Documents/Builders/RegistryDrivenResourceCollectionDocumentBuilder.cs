using System.Collections.Generic;
using System.Linq;
using JSONAPI.Core;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Builds a document for a collection of resources that are registered with a resource type registry
    /// </summary>
    public class RegistryDrivenResourceCollectionDocumentBuilder : RegistryDrivenDocumentBuilder, IResourceCollectionDocumentBuilder
    {
        /// <summary>
        /// Creates a new RegistryDrivenSingleResourceDocumentBuilder
        /// </summary>
        /// <param name="resourceTypeRegistry">The resource type registry to use to locate the registered type</param>
        /// <param name="linkConventions">Conventions to follow when building links</param>
        public RegistryDrivenResourceCollectionDocumentBuilder(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
            : base(resourceTypeRegistry, linkConventions)
        {
        }

        public IResourceCollectionDocument BuildDocument<TModel>(IEnumerable<TModel> primaryData, string linkBaseUrl, string[] includePathExpressions, IMetadata metadata,
            IDictionary<object, IMetadata> resourceMetadata = null)
        {
            var idDictionariesByType = new Dictionary<string, IDictionary<string, ResourceObject>>();
            var primaryDataResources =
                primaryData.Select(d => (IResourceObject)CreateResourceObject(d, idDictionariesByType, null, includePathExpressions, linkBaseUrl, resourceMetadata))
                    .ToArray();

            var primaryResourceIdentifiers = primaryDataResources.Select(r => new { r.Id, r.Type }).ToArray();

            var relatedData = idDictionariesByType.Values.SelectMany(d => d.Values).Cast<IResourceObject>().ToArray();
            var relatedDataNotInPrimaryData = relatedData
                .Where(r => !primaryResourceIdentifiers.Any(pri => pri.Id == r.Id && pri.Type == r.Type))
                .ToArray();

            var document = new ResourceCollectionDocument(primaryDataResources, relatedDataNotInPrimaryData, metadata);
            return document;
        }
    }
}