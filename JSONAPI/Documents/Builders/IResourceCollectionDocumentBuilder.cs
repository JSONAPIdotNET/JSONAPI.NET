using System.Collections.Generic;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Builds a response document from primary data objects
    /// </summary>
    public interface IResourceCollectionDocumentBuilder
    {
        /// <summary>
        /// Builds an IResourceCollectionDocument from the given queryable of model objects
        /// </summary>
        /// <param name="primaryData"></param>
        /// <param name="linkBaseUrl">The string to prepend to link URLs.</param>
        /// <param name="includePathExpressions">A list of dot-separated paths to include in the compound document.
        /// If this collection is null or empty, no linkage will be included.</param>
        /// <param name="metadata">Metadata for the top-level</param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        IResourceCollectionDocument BuildDocument<TModel>(IEnumerable<TModel> primaryData, string linkBaseUrl, string[] includePathExpressions, IMetadata metadata);
    }
}