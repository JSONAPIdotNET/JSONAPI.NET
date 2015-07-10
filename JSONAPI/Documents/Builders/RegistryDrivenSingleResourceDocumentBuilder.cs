using System.Collections.Generic;
using System.Linq;
using JSONAPI.Core;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Builds a document for a resource that is registered with a resource type registry
    /// </summary>
    public class RegistryDrivenSingleResourceDocumentBuilder : RegistryDrivenDocumentBuilder, ISingleResourceDocumentBuilder
    {
        /// <summary>
        /// Creates a new RegistryDrivenSingleResourceDocumentBuilder
        /// </summary>
        /// <param name="resourceTypeRegistry">The resource type registry to use to locate the registered type</param>
        /// <param name="linkConventions">Conventions to follow when building links</param>
        public RegistryDrivenSingleResourceDocumentBuilder(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
            : base(resourceTypeRegistry, linkConventions)
        {
        }

        public ISingleResourceDocument BuildDocument(object primaryData, string linkBaseUrl, string[] includePathExpressions, IMetadata topLevelMetadata)
        {
            var idDictionariesByType = new Dictionary<string, IDictionary<string, ResourceObject>>();
            var primaryDataResource = CreateResourceObject(primaryData, idDictionariesByType, null, includePathExpressions, linkBaseUrl);

            var relatedData = idDictionariesByType.Values.SelectMany(d => d.Values).Cast<IResourceObject>().ToArray();
            var document = new SingleResourceDocument(primaryDataResource, relatedData, topLevelMetadata);
            return document;
        }
    }
}