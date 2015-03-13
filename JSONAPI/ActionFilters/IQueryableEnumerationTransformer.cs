using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Provides a service to asynchronously materialize the results of an IQueryable into
    /// a concrete in-memory representation for serialization.
    /// </summary>
    public interface IQueryableEnumerationTransformer
    {
        /// <summary>
        /// Enumerates the specified query.
        /// </summary>
        /// <param name="query">The query to enumerate</param>
        /// <param name="cancellationToken">The request's cancellation token. If this token is cancelled during enumeration, enumeration must halt.</param>
        /// <typeparam name="T">The queryable element type</typeparam>
        /// <returns>A task yielding the enumerated results of the query</returns>
        Task<T[]> Enumerate<T>(IQueryable<T> query, CancellationToken cancellationToken);
    }
}
