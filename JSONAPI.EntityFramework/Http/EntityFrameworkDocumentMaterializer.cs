using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Extensions;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Http
{
    /// <summary>
    /// Implementation of IDocumentMaterializer for use with Entity Framework.
    /// </summary>
    public class EntityFrameworkDocumentMaterializer<T> : IDocumentMaterializer where T : class
    {
        protected readonly DbContext DbContext;
        private readonly IResourceTypeRegistration _resourceTypeRegistration;
        private readonly IQueryableResourceCollectionDocumentBuilder _queryableResourceCollectionDocumentBuilder;
        private readonly ISingleResourceDocumentBuilder _singleResourceDocumentBuilder;
        private readonly IEntityFrameworkResourceObjectMaterializer _entityFrameworkResourceObjectMaterializer;
        private readonly ISortExpressionExtractor _sortExpressionExtractor;
        private readonly IIncludeExpressionExtractor _includeExpressionExtractor;
        private readonly IBaseUrlService _baseUrlService;

        /// <summary>
        /// Creates a new EntityFrameworkDocumentMaterializer
        /// </summary>
        public EntityFrameworkDocumentMaterializer(
            DbContext dbContext,
            IResourceTypeRegistration resourceTypeRegistration,
            IQueryableResourceCollectionDocumentBuilder queryableResourceCollectionDocumentBuilder,
            ISingleResourceDocumentBuilder singleResourceDocumentBuilder,
            IEntityFrameworkResourceObjectMaterializer entityFrameworkResourceObjectMaterializer,
            ISortExpressionExtractor sortExpressionExtractor,
            IIncludeExpressionExtractor includeExpressionExtractor,
            IBaseUrlService baseUrlService)
        {
            DbContext = dbContext;
            _resourceTypeRegistration = resourceTypeRegistration;
            _queryableResourceCollectionDocumentBuilder = queryableResourceCollectionDocumentBuilder;
            _singleResourceDocumentBuilder = singleResourceDocumentBuilder;
            _entityFrameworkResourceObjectMaterializer = entityFrameworkResourceObjectMaterializer;
            _sortExpressionExtractor = sortExpressionExtractor;
            _includeExpressionExtractor = includeExpressionExtractor;
            _baseUrlService = baseUrlService;
        }

        public virtual Task<IResourceCollectionDocument> GetRecords(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var sortExpressions = _sortExpressionExtractor.ExtractSortExpressions(request);
            var includes = _includeExpressionExtractor.ExtractIncludeExpressions(request);
            var query = QueryIncludeNavigationProperties(null, GetNavigationPropertiesIncludes<T>(includes));
            return _queryableResourceCollectionDocumentBuilder.BuildDocument(query, request, sortExpressions, cancellationToken, includes);
        }

        public virtual async Task<ISingleResourceDocument> GetRecordById(string id, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var includes = _includeExpressionExtractor.ExtractIncludeExpressions(request);
            var singleResource = await FilterById(id, _resourceTypeRegistration, GetNavigationPropertiesIncludes<T>(includes)).FirstOrDefaultAsync(cancellationToken);
            if (singleResource == null)
                throw JsonApiException.CreateForNotFound(string.Format("No resource of type `{0}` exists with id `{1}`.",
                    _resourceTypeRegistration.ResourceTypeName, id));
            return _singleResourceDocumentBuilder.BuildDocument(singleResource, apiBaseUrl, includes, null);
        }

        public virtual async Task<ISingleResourceDocument> CreateRecord(ISingleResourceDocument requestDocument,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var newRecord = MaterializeAsync(requestDocument.PrimaryData, cancellationToken);
            await OnCreate(newRecord);
            await DbContext.SaveChangesAsync(cancellationToken);
            var includes = _includeExpressionExtractor.ExtractIncludeExpressions(request);
            var returnDocument = _singleResourceDocumentBuilder.BuildDocument(await newRecord, apiBaseUrl, includes, null);

            return returnDocument;
        }


        public virtual async Task<ISingleResourceDocument> UpdateRecord(string id, ISingleResourceDocument requestDocument,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var newRecord = MaterializeAsync(requestDocument.PrimaryData, cancellationToken);
            await OnUpdate(newRecord);
            await DbContext.SaveChangesAsync(cancellationToken);
            var includes = _includeExpressionExtractor.ExtractIncludeExpressions(request);
            var returnDocument = _singleResourceDocumentBuilder.BuildDocument(await newRecord, apiBaseUrl, includes, null);

            return returnDocument;
        }

        public virtual async Task<IJsonApiDocument> DeleteRecord(string id, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var singleResource = DbContext.Set<T>().FindAsync(cancellationToken, Convert.ChangeType(id, _resourceTypeRegistration.IdProperty.PropertyType));
            await OnDelete(singleResource);
            DbContext.Set<T>().Remove(await singleResource);
            await DbContext.SaveChangesAsync(cancellationToken);

            return null;
        }

        /// <summary>
        /// Gets the base URL for link creation from the current request
        /// </summary>
        protected string GetBaseUrlFromRequest(HttpRequestMessage request)
        {
            return _baseUrlService.GetBaseUrl(request);
        }

        /// <summary>
        /// Convert a resource object into a material record managed by EntityFramework.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<T> MaterializeAsync(IResourceObject resourceObject, CancellationToken cancellationToken)
        {
            return (T) await _entityFrameworkResourceObjectMaterializer.MaterializeResourceObject(resourceObject, cancellationToken);
        }

      
        /// <summary>
        /// Manipulate entity before create.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        protected virtual async Task OnCreate(Task<T> record)
        {
            await record;
        }

        /// <summary>
        /// Manipulate entity before update.
        /// </summary>
        /// <param name="record"></param>
        protected virtual async Task OnUpdate(Task<T> record)
        {
            await record;
        }

        /// <summary>
        /// Manipulate entity before delete.
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        protected virtual async Task OnDelete(Task<T> record)
        {
            await record;
        }

        /// <summary>
        /// This method allows to include <see cref="QueryableExtensions.Include{T}"/> into query.
        /// This can reduce the number of queries (eager loading)
        /// </summary>
        /// <typeparam name="TResource"></typeparam>
        /// <param name="includes"></param>
        /// <returns></returns>
        protected virtual Expression<Func<TResource, object>>[] GetNavigationPropertiesIncludes<TResource>(string[] includes)
        {
            List<Expression<Func<TResource, object>>> list = new List<Expression<Func<TResource, object>>>();
            foreach (var include in includes)
            {
                var incl = include.Pascalize();
                var param = Expression.Parameter(typeof(TResource));
                var lambda =
                    Expression.Lambda<Func<TResource, object>>(
                        Expression.PropertyOrField(param, incl),param);
                    list.Add(lambda);
            }
            return list.ToArray();
        }


        private IQueryable<TResource> QueryIncludeNavigationProperties<TResource>(Expression<Func<TResource, bool>> predicate,
            params Expression<Func<TResource, object>>[] includes) where TResource : class
        {
            IQueryable<TResource> query = DbContext.Set<TResource>();
            if (includes != null && includes.Any()) // eager loading
                query = includes.Aggregate(query, (current, include) => current.Include(include));

            if (predicate != null)
                query = query.Where(predicate);

            return query.AsQueryable();
        }

        private IQueryable<TResource> FilterById<TResource>(string id, IResourceTypeRegistration resourceTypeRegistration,
            params Expression<Func<TResource, object>>[] includes) where TResource : class
        {
            var param = Expression.Parameter(typeof(TResource));
            var filterByIdExpression = resourceTypeRegistration.GetFilterByIdExpression(param, id);
            var predicate = Expression.Lambda<Func<TResource, bool>>(filterByIdExpression, param);
            return QueryIncludeNavigationProperties(predicate, includes);
        }
    }
}
