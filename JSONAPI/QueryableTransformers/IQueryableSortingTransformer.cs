using System.Linq;
using System.Net.Http;

namespace JSONAPI.QueryableTransformers
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
        /// <param name="sortExpressions">The expressions to sort by</param>
        /// <typeparam name="T">The element type of the query</typeparam>
        /// <returns>The sorted query</returns>
        IOrderedQueryable<T> Sort<T>(IQueryable<T> query, string[] sortExpressions);
    }
}