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

        public async Task<IResourceCollectionDocument> GetRecords(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var entityQuery = GetQuery();
            var includePaths = GetIncludePathsForQuery() ?? new Expression<Func<TDto, object>>[] { };
            var jsonApiPaths = includePaths.Select(ConvertToJsonKeyPath).ToArray();
            var mappedQuery = GetMappedQuery(entityQuery, includePaths);
            return await _queryableResourceCollectionDocumentBuilder.BuildDocument(mappedQuery, request, cancellationToken, jsonApiPaths);
        }

        public async Task<ISingleResourceDocument> GetRecordById(string id, HttpRequestMessage request, CancellationToken cancellationToken)
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

        private class PathVisitor : ExpressionVisitor
        {
            private readonly IResourceTypeRegistry _resourceTypeRegistry;

            public PathVisitor(IResourceTypeRegistry resourceTypeRegistry)
            {
                _resourceTypeRegistry = resourceTypeRegistry;
            }

            private readonly Stack<string> _segments = new Stack<string>();
            public string Path { get { return string.Join(".", _segments.ToArray()); } }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == "Select")
                {
                    Visit(node.Arguments[1]);
                    Visit(node.Arguments[0]);
                }
                return node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var property = node.Member as PropertyInfo;
                if (property == null) return node;

                var registration = _resourceTypeRegistry.GetRegistrationForType(property.DeclaringType);
                if (registration == null || registration.Relationships == null) return node;

                var relationship = registration.Relationships.FirstOrDefault(r => r.Property == property);
                if (relationship == null) return node;

                _segments.Push(relationship.JsonKey);

                return base.VisitMember(node);
            }
        }
    }
}
