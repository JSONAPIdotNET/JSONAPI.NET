using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Default implementation of IResourceObject
    /// </summary>
    public class ResourceObject : IResourceObject
    {
        public string Type { get; private set; }
        public string Id { get; private set; }
        public IDictionary<string, JToken> Attributes { get; private set; }
        public IDictionary<string, IRelationshipObject> Relationships { get; private set; }
        public ILink SelfLink { get; private set; }
        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Creates a ResourceObject
        /// </summary>
        public ResourceObject(string type, string id, IDictionary<string, JToken> attributes = null,
            IDictionary<string, IRelationshipObject> relationships = null, ILink selfLink = null, IMetadata metadata = null)
        {
            Type = type;
            Id = id;
            Attributes = attributes ?? new Dictionary<string, JToken>();
            Relationships = relationships ?? new Dictionary<string, IRelationshipObject>();
            SelfLink = selfLink;
            Metadata = metadata;
        }
    }
}