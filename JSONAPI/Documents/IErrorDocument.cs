namespace JSONAPI.Documents
{
    /// <summary>
    /// Interface for JSON API documents that represent a collection of errors
    /// </summary>
    public interface IErrorDocument : IJsonApiDocument
    {
        /// <summary>
        /// The errors to send in this document
        /// </summary>
        IError[] Errors { get; }
    }
}