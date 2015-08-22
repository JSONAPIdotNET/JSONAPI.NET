namespace JSONAPI.Documents
{
    /// <summary>
    /// Interface for JSON API documents that represent a collection of resources
    /// </summary>
    public interface IResourceCollectionDocument : IJsonApiDocument
    {
        /// <summary>
        /// The document's primary data
        /// </summary>
        IResourceObject[] PrimaryData { get; }

        /// <summary>
        /// Data related to the primary data
        /// </summary>
        IResourceObject[] RelatedData { get; }
    }
}