namespace JSONAPI.Documents
{
    /// <summary>
    /// Default implementation for IRelationshipObject
    /// </summary>
    public class RelationshipObject : IRelationshipObject
    {
        public ILink SelfLink { get; private set; }
        public ILink RelatedResourceLink { get; private set; }
        public IResourceLinkage Linkage { get; private set; }
        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Builds a new RelationshipObject with links only
        /// </summary>
        public RelationshipObject(ILink selfLink, ILink relatedResourceLink, IMetadata metadata = null)
        {
            SelfLink = selfLink;
            RelatedResourceLink = relatedResourceLink;
            Metadata = metadata;
        }
        
        /// <summary>
        /// Builds a new RelationshipObject with linkage only
        /// </summary>
        public RelationshipObject(IResourceLinkage linkage, IMetadata metadata = null)
        {
            Linkage = linkage;
            Metadata = metadata;
        }

        /// <summary>
        /// Builds a new RelationshipObject with links and linkage
        /// </summary>
        public RelationshipObject(IResourceLinkage linkage, ILink selfLink, ILink relatedResourceLink, IMetadata metadata = null)
        {
            Linkage = linkage;
            SelfLink = selfLink;
            RelatedResourceLink = relatedResourceLink;
            Metadata = metadata;
        }
    }
}
