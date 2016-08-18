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
        private readonly ISortExpressionExtractor _sortExpressionExtractor;
        private readonly IIncludeExpressionExtractor _includeExpressionExtractor;

        /// <summary>
        /// Creates a new QueryableRelatedResourceDocumentMaterializer
        /// </summary>
        protected QueryableToManyRelatedResourceDocumentMaterializer(
            IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder,
            ISortExpressionExtractor sortExpressionExtractor,
            IIncludeExpressionExtractor includeExpressionExtractor)
        {
            _queryableResourceCollectionDocumentBuilder = queryableResourceCollectionDocumentBuilder;
            _sortExpressionExtractor = sortExpressionExtractor;
            _includeExpressionExtractor = includeExpressionExtractor;
        }

        public async Task<IJsonApiDocument> GetRelatedResourceDocument(string primaryResourceId, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var query = await GetRelatedQuery(primaryResourceId, cancellationToken);
            var includes = _includeExpressionExtractor.ExtractIncludeExpressions(request);
            var sortExpressions = _sortExpressionExtractor.ExtractSortExpressions(request);


            return await _queryableResourceCollectionDocumentBuilder.BuildDocument(query, request, sortExpressions, cancellationToken, includes); // TODO: allow implementors to specify metadata
        }

        /// <summary>
        /// Gets the query for the related resources
        /// </summary>
        protected abstract Task<IQueryable<TRelated>> GetRelatedQuery(string primaryResourceId, CancellationToken cancellationToken);

        /// <summary>
        /// If the client doesn't request any sort expressions, these expressions will be used for sorting instead.
        /// </summary>
        /// <returns></returns>
        protected virtual string[] GetDefaultSortExpressions()
        {
            return null;
        }
    }
}