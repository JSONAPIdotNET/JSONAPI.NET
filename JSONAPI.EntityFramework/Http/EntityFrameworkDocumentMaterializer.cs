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
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Http
{
    /// <summary>
    /// Implementation of IDocumentMaterializer for use with Entity Framework.
    /// </summary>
    public class EntityFrameworkDocumentMaterializer<T> : IDocumentMaterializer where T : class
    {
        private readonly DbContext _dbContext;
        private readonly IResourceTypeRegistration _resourceTypeRegistration;
        private readonly IQueryableResourceCollectionDocumentBuilder _queryableResourceCollectionDocumentBuilder;
        private readonly ISingleResourceDocumentBuilder _singleResourceDocumentBuilder;
        private readonly IEntityFrameworkResourceObjectMaterializer _entityFrameworkResourceObjectMaterializer;
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
            IBaseUrlService baseUrlService)
        {
            _dbContext = dbContext;
            _resourceTypeRegistration = resourceTypeRegistration;
            _queryableResourceCollectionDocumentBuilder = queryableResourceCollectionDocumentBuilder;
            _singleResourceDocumentBuilder = singleResourceDocumentBuilder;
            _entityFrameworkResourceObjectMaterializer = entityFrameworkResourceObjectMaterializer;
            _baseUrlService = baseUrlService;
        }

        public virtual Task<IResourceCollectionDocument> GetRecords(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<T>().AsQueryable();
            return _queryableResourceCollectionDocumentBuilder.BuildDocument(query, request, cancellationToken);
        }

        public virtual async Task<ISingleResourceDocument> GetRecordById(string id, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var singleResource = await FilterById<T>(id, _resourceTypeRegistration).FirstOrDefaultAsync(cancellationToken);
            if (singleResource == null)
                throw JsonApiException.CreateForNotFound(string.Format("No resource of type `{0}` exists with id `{1}`.",
                    _resourceTypeRegistration.ResourceTypeName, id));
            return _singleResourceDocumentBuilder.BuildDocument(singleResource, apiBaseUrl, null, null);
        }

        public virtual async Task<ISingleResourceDocument> CreateRecord(ISingleResourceDocument requestDocument,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var newRecord = await MaterializeAsync(requestDocument.PrimaryData, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var returnDocument = _singleResourceDocumentBuilder.BuildDocument(newRecord, apiBaseUrl, null, null);

            return returnDocument;
        }

        public virtual async Task<ISingleResourceDocument> UpdateRecord(string id, ISingleResourceDocument requestDocument,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var newRecord = await MaterializeAsync(requestDocument.PrimaryData, cancellationToken);
            var returnDocument = _singleResourceDocumentBuilder.BuildDocument(newRecord, apiBaseUrl, null, null);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return returnDocument;
        }

        public virtual async Task<IJsonApiDocument> DeleteRecord(string id, CancellationToken cancellationToken)
        {
            var singleResource = await _dbContext.Set<T>().FindAsync(cancellationToken, id);
            _dbContext.Set<T>().Remove(singleResource);
            await _dbContext.SaveChangesAsync(cancellationToken);

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
        protected virtual async Task<object> MaterializeAsync(IResourceObject resourceObject, CancellationToken cancellationToken)
        {
            return await _entityFrameworkResourceObjectMaterializer.MaterializeResourceObject(resourceObject, cancellationToken);
        }

        /// <summary>
        /// Generic method for getting the related resources for a to-many relationship
        /// </summary>
        protected async Task<IResourceCollectionDocument> GetRelatedToMany<TRelated>(string id,
            ResourceTypeRelationship relationship, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var param = Expression.Parameter(typeof(T));
            var accessorExpr = Expression.Property(param, relationship.Property);
            var lambda = Expression.Lambda<Func<T, IEnumerable<TRelated>>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<T>(id, _resourceTypeRegistration);

            // We have to see if the resource even exists, so we can throw a 404 if it doesn't
            var relatedResource = await primaryEntityQuery.FirstOrDefaultAsync(cancellationToken);
            if (relatedResource == null)
                throw JsonApiException.CreateForNotFound(string.Format("No resource of type `{0}` exists with id `{1}`.",
                    _resourceTypeRegistration.ResourceTypeName, id));

            var relatedResourceQuery = primaryEntityQuery.SelectMany(lambda);

            return await _queryableResourceCollectionDocumentBuilder.BuildDocument(relatedResourceQuery, request, cancellationToken);
        }

        /// <summary>
        /// Generic method for getting the related resources for a to-one relationship
        /// </summary>
        protected async Task<ISingleResourceDocument> GetRelatedToOne<TRelated>(string id,
            ResourceTypeRelationship relationship, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var param = Expression.Parameter(typeof(T));
            var accessorExpr = Expression.Property(param, relationship.Property);
            var lambda = Expression.Lambda<Func<T, TRelated>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<T>(id, _resourceTypeRegistration);
            var primaryEntityExists = await primaryEntityQuery.AnyAsync(cancellationToken);
            if (!primaryEntityExists)
                throw JsonApiException.CreateForNotFound(string.Format("No resource of type `{0}` exists with id `{1}`.",
                    _resourceTypeRegistration.ResourceTypeName, id));
            var relatedResource = await primaryEntityQuery.Select(lambda).FirstOrDefaultAsync(cancellationToken);
            return _singleResourceDocumentBuilder.BuildDocument(relatedResource, GetBaseUrlFromRequest(request), null, null);
        }

        private IQueryable<TResource> Filter<TResource>(Expression<Func<TResource, bool>> predicate,
            params Expression<Func<TResource, object>>[] includes) where TResource : class
        {
            IQueryable<TResource> query = _dbContext.Set<TResource>();
            if (includes != null && includes.Any())
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
            return Filter(predicate, includes);
        }
    }
}
