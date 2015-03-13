using System.Linq;
using System.Net.Http;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Provides a service to provide a page of data based on information from the request.
    /// </summary>
    public interface IQueryablePaginationTransformer
    {
        /// <summary>
        /// Pages the query according to information from the request.
        /// </summary>
        /// <param name="query">The query to page</param>
        /// <param name="request">The request message</param>
        /// <typeparam name="T">The queryable element type</typeparam>
        /// <returns>An IQueryable configured to return a page of data</returns>
        IQueryable<T> ApplyPagination<T>(IQueryable<T> query, HttpRequestMessage request);
    }
}
