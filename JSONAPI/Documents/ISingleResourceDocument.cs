namespace JSONAPI.Documents
{
    /// <summary>
    /// Interface for JSON API documents that represent a single resource
    /// </summary>
    public interface ISingleResourceDocument : IJsonApiDocument
    {
        /// <summary>
        /// The document's primary data
        /// </summary>
        IResourceObject PrimaryData { get; }

        /// <summary>
        /// Data related to the primary data
        /// </summary>
        IResourceObject[] RelatedData { get; }
    }
}