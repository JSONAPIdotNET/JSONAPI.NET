using System.Collections.Generic;
using System.Linq;
using JSONAPI.Core;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Builds a payload for a collection of resources that are registered with a resource type registry
    /// </summary>
    public class RegistryDrivenResourceCollectionPayloadBuilder : RegistryDrivenPayloadBuilder, IResourceCollectionPayloadBuilder
    {
        /// <summary>
        /// Creates a new RegistryDrivenSingleResourcePayloadBuilder
        /// </summary>
        /// <param name="resourceTypeRegistry">The resource type registry to use to locate the registered type</param>
        /// <param name="linkConventions">Conventions to follow when building links</param>
        public RegistryDrivenResourceCollectionPayloadBuilder(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
            : base(resourceTypeRegistry, linkConventions)
        {
        }

        public IResourceCollectionPayload BuildPayload<TModel>(IEnumerable<TModel> primaryData, string linkBaseUrl, string[] includePathExpressions)
        {
            var idDictionariesByType = new Dictionary<string, IDictionary<string, ResourceObject>>();
            var primaryDataResources =
                primaryData.Select(d => (IResourceObject)CreateResourceObject(d, idDictionariesByType, null, includePathExpressions, linkBaseUrl))
                    .ToArray();

            var relatedData = idDictionariesByType.Values.SelectMany(d => d.Values).Cast<IResourceObject>().ToArray();
            var payload = new ResourceCollectionPayload(primaryDataResources, relatedData, null);
            return payload;
        }
    }
}