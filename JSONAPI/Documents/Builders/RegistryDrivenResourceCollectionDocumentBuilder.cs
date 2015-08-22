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

        public IResourceCollectionDocument BuildDocument<TModel>(IEnumerable<TModel> primaryData, string linkBaseUrl, string[] includePathExpressions, IMetadata metadata)
        {
            var idDictionariesByType = new Dictionary<string, IDictionary<string, ResourceObject>>();
            var primaryDataResources =
                primaryData.Select(d => (IResourceObject)CreateResourceObject(d, idDictionariesByType, null, includePathExpressions, linkBaseUrl))
                    .ToArray();

            var relatedData = idDictionariesByType.Values.SelectMany(d => d.Values).Cast<IResourceObject>().ToArray();
            var document = new ResourceCollectionDocument(primaryDataResources, relatedData, metadata);
            return document;
        }
    }
}