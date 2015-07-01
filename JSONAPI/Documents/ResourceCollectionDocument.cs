namespace JSONAPI.Documents
{
    /// <summary>
    /// Default implementation of IResourceCollectionDocument
    /// </summary>
    public class ResourceCollectionDocument : IResourceCollectionDocument
    {
        public IResourceObject[] PrimaryData { get; private set; }
        public IResourceObject[] RelatedData { get; private set; }
        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Constructs a resource collection document
        /// </summary>
        public ResourceCollectionDocument(IResourceObject[] primaryData, IResourceObject[] relatedData, IMetadata metadata)
        {
            PrimaryData = primaryData;
            RelatedData = relatedData;
            Metadata = metadata;
        }
    }
}
