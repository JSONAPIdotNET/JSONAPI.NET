using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Represents a JSON API resource object
    /// </summary>
    public interface IResourceObject
    {
        /// <summary>
        /// The type of the resource
        /// </summary>
        string Type { get; }

        /// <summary>
        /// The ID of the resource
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Attributes of the resource
        /// </summary>
        IDictionary<string, JToken> Attributes { get; }

        /// <summary>
        /// Relationships between the resource and other JSON API resources
        /// </summary>
        IDictionary<string, IRelationshipObject> Relationships { get; }

        /// <summary>
        /// A link to the resource
        /// </summary>
        ILink SelfLink { get; }

        /// <summary>
        /// Metadata about this particular resource
        /// </summary>
        IMetadata Metadata { get; }
    }
}