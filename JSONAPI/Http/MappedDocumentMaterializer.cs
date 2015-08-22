using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.Http
{
    /// <summary>
    /// Document materializer for mapping from a database entity to a data transfer object.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TDto"></typeparam>
    public abstract class MappedDocumentMaterializer<TEntity, TDto> : IDocumentMaterializer where TDto : class
    {
        private readonly IQueryableResourceCollectionDocumentBuilder _queryableResourceCollectionDocumentBuilder;
        private readonly IBaseUrlService _baseUrlService;
        private readonly ISingleResourceDocumentBuilder _singleResourceDocumentBuilder;
        private readonly IQueryableEnumerationTransformer _queryableEnumerationTransformer;
        private readonly IResourceTypeRegistry _resourceTypeRegistry;

        /// <summary>
        /// Gets a query returning all entities for this endpoint
        /// </summary>
        protected abstract IQueryable<TEntity> GetQuery();

        /// <summary>
        /// Gets a query for only the entity matching the given ID
        /// </summary>
        protected abstract IQueryable<TEntity> GetByIdQuery(string id);

        /// <summary>
        /// Gets a query for the DTOs based on the given entity query.
        /// </summary>
        protected abstract IQueryable<TDto> GetMappedQuery(IQueryable<TEntity> entityQuery, Expression<Func<TDto, object>>[] propertiesToInclude);

        /// <summary>
        /// Creates a new MappedDocumentMaterializer
        /// </summary>
        protected MappedDocumentMaterializer(
            IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder,
            IBaseUrlService baseUrlService,
            ISingleResourceDocumentBuilder singleResourceDocumentBuilder,
            IQueryableEnumerationTransformer queryableEnumerationTransformer,
            IResourceTypeRegistry resourceTypeRegistry)
        {
            _queryableResourceCollectionDocumentBuilder = queryableResourceCollectionDocumentBuilder;
            _baseUrlService = baseUrlService;
            _singleResourceDocumentBuilder = singleResourceDocumentBuilder;
            _queryableEnumerationTransformer = queryableEnumerationTransformer;
            _resourceTypeRegistry = resourceTypeRegistry;
        }

        private string ResourceTypeName
        {
            get { return _resourceTypeRegistry.GetRegistrationForType(typeof (TDto)).ResourceTypeName; }
        }

        public virtual async Task<IResourceCollectionDocument> GetRecords(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var entityQuery = GetQuery();
            var includePaths = GetIncludePathsForQuery() ?? new Expression<Func<TDto, object>>[] { };
            var jsonApiPaths = includePaths.Select(ConvertToJsonKeyPath).ToArray();
            var mappedQuery = GetMappedQuery(entityQuery, includePaths);
            return await _queryableResourceCollectionDocumentBuilder.BuildDocument(mappedQuery, request, cancellationToken, jsonApiPaths);
        }

        public virtual async Task<ISingleResourceDocument> GetRecordById(string id, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var entityQuery = GetByIdQuery(id);
            var includePaths = GetIncludePathsForSingleResource() ?? new Expression<Func<TDto, object>>[] { };
            var jsonApiPaths = includePaths.Select(ConvertToJsonKeyPath).ToArray();
            var mappedQuery = GetMappedQuery(entityQuery, includePaths);
            var primaryResource = await _queryableEnumerationTransformer.FirstOrDefault(mappedQuery, cancellationToken);
            if (primaryResource == null)
                throw JsonApiException.CreateForNotFound(
                    string.Format("No record exists with type `{0}` and ID `{1}`.", ResourceTypeName, id));

            var baseUrl = _baseUrlService.GetBaseUrl(request);
            return _singleResourceDocumentBuilder.BuildDocument(primaryResource, baseUrl, jsonApiPaths, null);
        }

        public abstract Task<ISingleResourceDocument> CreateRecord(ISingleResourceDocument requestDocument,
            HttpRequestMessage request,
            CancellationToken cancellationToken);

        public abstract Task<ISingleResourceDocument> UpdateRecord(string id, ISingleResourceDocument requestDocument,
            HttpRequestMessage request,
            CancellationToken cancellationToken);

        public abstract Task<IJsonApiDocument> DeleteRecord(string id, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a list of property paths to be included when constructing a query for this resource type
        /// </summary>
        protected virtual Expression<Func<TDto, object>>[] GetIncludePathsForQuery()
        {
            return null;
        }

        /// <summary>
        /// Returns a list of property paths to be included when returning a single resource of this resource type
        /// </summary>
        protected virtual Expression<Func<TDto, object>>[] GetIncludePathsForSingleResource()
        {
            return null;
        }

        private string ConvertToJsonKeyPath(Expression<Func<TDto, object>> expression)
        {
            var visitor = new PathVisitor(_resourceTypeRegistry);
            visitor.Visit(expression);
            return visitor.Path;
        }
    }
}
