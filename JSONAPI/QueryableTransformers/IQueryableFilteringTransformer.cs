using System.Linq;
using System.Net.Http;

namespace JSONAPI.QueryableTransformers
{
    /// <summary>
    /// Service for filtering an IQueryable according to an HTTP request.
    /// </summary>
    public interface IQueryableFilteringTransformer
    {
        /// <summary>
        /// Filters the provided queryable based on information from the request message.
        /// </summary>
        /// <param name="query">The input query</param>
        /// <param name="request">The request message</param>
        /// <typeparam name="T">The element type of the query</typeparam>
        /// <returns>The filtered query</returns>
        IQueryable<T> Filter<T>(IQueryable<T> query, HttpRequestMessage request);
    }
}
