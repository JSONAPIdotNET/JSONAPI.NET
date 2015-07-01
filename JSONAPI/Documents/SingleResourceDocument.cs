namespace JSONAPI.Documents
{
    /// <summary>
    /// Default implementation of ISingleResourceDocument
    /// </summary>
    public class SingleResourceDocument : ISingleResourceDocument
    {
        public IResourceObject PrimaryData { get; private set; }

        public IResourceObject[] RelatedData { get; private set; }

        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Constructs a single resource document
        /// </summary>
        public SingleResourceDocument(IResourceObject primaryData, IResourceObject[] relatedData, IMetadata metadata)
        {
            PrimaryData = primaryData;
            RelatedData = relatedData;
            Metadata = metadata;
        }
    }
}
