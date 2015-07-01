namespace JSONAPI.Documents
{
    /// <summary>
    /// Represents a JSON API relationship object
    /// </summary>
    public interface IRelationshipObject
    {
        /// <summary>
        /// A link for the relationship itself
        /// </summary>
        ILink SelfLink { get; }

        /// <summary>
        /// A link for the related resource
        /// </summary>
        ILink RelatedResourceLink { get; }

        /// <summary>
        /// Linkage to the resources defined by this relationship
        /// </summary>
        IResourceLinkage Linkage { get; }

        /// <summary>
        /// Metadata for the relationship
        /// </summary>
        IMetadata Metadata { get; }
    }
}