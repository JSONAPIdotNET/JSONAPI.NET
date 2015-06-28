using System.Collections.Generic;
using System.Linq;
using JSONAPI.Core;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Builds a payload for a resource that is registered with a resource type registry
    /// </summary>
    public class RegistryDrivenSingleResourcePayloadBuilder : RegistryDrivenPayloadBuilder, ISingleResourcePayloadBuilder
    {
        /// <summary>
        /// Creates a new RegistryDrivenSingleResourcePayloadBuilder
        /// </summary>
        /// <param name="resourceTypeRegistry">The resource type registry to use to locate the registered type</param>
        /// <param name="linkConventions">Conventions to follow when building links</param>
        public RegistryDrivenSingleResourcePayloadBuilder(IResourceTypeRegistry resourceTypeRegistry, ILinkConventions linkConventions)
            : base(resourceTypeRegistry, linkConventions)
        {
        }

        public ISingleResourcePayload BuildPayload(object primaryData, string linkBaseUrl, string[] includePathExpressions)
        {
            var idDictionariesByType = new Dictionary<string, IDictionary<string, ResourceObject>>();
            var primaryDataResource = CreateResourceObject(primaryData, idDictionariesByType, null, includePathExpressions, linkBaseUrl);

            var relatedData = idDictionariesByType.Values.SelectMany(d => d.Values).Cast<IResourceObject>().ToArray();
            var payload = new SingleResourcePayload(primaryDataResource, relatedData, null);
            return payload;
        }
    }
}