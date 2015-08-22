using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// This interface is responsible for building IResourceCollectionDocument objects based on IQueryable ObjectContent
    /// </summary>
    public interface IQueryableResourceCollectionDocumentBuilder
    {
        /// <summary>
        /// Builds a document object for the given query
        /// </summary>
        /// <param name="query">The query to materialize to build the response document</param>
        /// <param name="request">The request containing parameters to determine how to sort/filter/paginate the query</param>
        /// <param name="cancellationToken"></param>
        /// <param name="includePaths">The set of paths to include in the compound document</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IResourceCollectionDocument> BuildDocument<T>(IQueryable<T> query, HttpRequestMessage request, CancellationToken cancellationToken,
            string[] includePaths = null);
    }
}
