using System.Collections.Generic;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Builds a response payload from primary data objects
    /// </summary>
    public interface IResourceCollectionPayloadBuilder
    {
        /// <summary>
        /// Builds an IResourceCollectionPayload from the given queryable of model objects
        /// </summary>
        /// <param name="primaryData"></param>
        /// <param name="linkBaseUrl">The string to prepend to link URLs.</param>
        /// <param name="includePathExpressions">A list of dot-separated paths to include in the compound document.
        /// If this collection is null or empty, no linkage will be included.</param>
        /// <typeparam name="TModel"></typeparam>
        /// <returns></returns>
        IResourceCollectionPayload BuildPayload<TModel>(IEnumerable<TModel> primaryData, string linkBaseUrl, string[] includePathExpressions);
    }
}