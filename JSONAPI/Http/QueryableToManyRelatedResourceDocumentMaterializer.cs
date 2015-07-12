using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;

namespace JSONAPI.Http
{
    /// <summary>
    /// Base class for implementations of <see cref="IRelatedResourceDocumentMaterializer"/> that use IQueryable to get related resources
    /// for a to-many relationship.
    /// </summary>
    public abstract class QueryableToManyRelatedResourceDocumentMaterializer<TRelated> : IRelatedResourceDocumentMaterializer
    {
        private readonly IQueryableResourceCollectionDocumentBuilder _queryableResourceCollectionDocumentBuilder;

        /// <summary>
        /// Creates a new QueryableRelatedResourceDocumentMaterializer
        /// </summary>
        protected QueryableToManyRelatedResourceDocumentMaterializer(IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder)
        {
            _queryableResourceCollectionDocumentBuilder = queryableResourceCollectionDocumentBuilder;
        }

        public async Task<IJsonApiDocument> GetRelatedResourceDocument(string primaryResourceId, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var query = await GetRelatedQuery(primaryResourceId, cancellationToken);
            return await _queryableResourceCollectionDocumentBuilder.BuildDocument(query, request, cancellationToken); // TODO: allow implementors to specify includes and metadata
        }

        /// <summary>
        /// Gets the query for the related resources
        /// </summary>
        protected abstract Task<IQueryable<TRelated>> GetRelatedQuery(string primaryResourceId, CancellationToken cancellationToken);
    }
}