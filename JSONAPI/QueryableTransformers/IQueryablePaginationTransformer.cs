using System.Linq;
using System.Net.Http;

namespace JSONAPI.QueryableTransformers
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
        /// <returns>The result of pagination</returns>
        IPaginationTransformResult<T> ApplyPagination<T>(IQueryable<T> query, HttpRequestMessage request);
    }

    /// <summary>
    /// The result of a pagination transform
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPaginationTransformResult<out T>
    {
        /// <summary>
        /// A query that has been paginated
        /// </summary>
        IQueryable<T> PagedQuery { get; }

        /// <summary>
        /// Whether the query has been paginated or not
        /// </summary>
        bool PaginationWasApplied { get; }

        /// <summary>
        /// The current page of the query
        /// </summary>
        int PageNumber { get; }

        /// <summary>
        /// The size of this page of data
        /// </summary>
        int PageSize { get; }
    }
}
