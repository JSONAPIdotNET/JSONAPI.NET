namespace JSONAPI.Documents
{
    /// <summary>
    /// Base interface for document
    /// </summary>
    public interface IJsonApiDocument
    {
        /// <summary>
        /// Metadata for the document as a whole
        /// </summary>
        IMetadata Metadata { get; }
    }
}