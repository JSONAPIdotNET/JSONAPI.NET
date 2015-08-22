namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Builds a response document from primary data objects
    /// </summary>
    public interface ISingleResourceDocumentBuilder
    {
        /// <summary>
        /// Builds an ISingleResourceDocument from the given model object
        /// </summary>
        /// <param name="primaryData"></param>
        /// <param name="linkBaseUrl">The string to prepend to link URLs.</param>
        /// <param name="includePathExpressions">A list of dot-separated paths to include in the compound document.
        /// If this collection is null or empty, no linkage will be included.</param>
        /// <param name="topLevelMetadata">Metadata to serialize at the top level of the document</param>
        /// <returns></returns>
        ISingleResourceDocument BuildDocument(object primaryData, string linkBaseUrl, string[] includePathExpressions, IMetadata topLevelMetadata);
    }
}
