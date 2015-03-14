using System.Linq;
using System.Net.Http;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Service for sorting an IQueryable according to an HTTP request.
    /// </summary>
    public interface IQueryableSortingTransformer
    {
        /// <summary>
        /// Sorts the provided queryable based on information from the request message.
        /// </summary>
        /// <param name="query">The input query</param>
        /// <param name="request">The request message</param>
        /// <typeparam name="T">The element type of the query</typeparam>
        /// <returns>The sorted query</returns>
        IOrderedQueryable<T> Sort<T>(IQueryable<T> query, HttpRequestMessage request);
    }
}