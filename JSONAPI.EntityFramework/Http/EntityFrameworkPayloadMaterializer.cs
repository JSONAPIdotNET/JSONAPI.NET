using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Core;
using JSONAPI.Http;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;

namespace JSONAPI.EntityFramework.Http
{
    /// <summary>
    /// Implementation of IPayloadMaterializer for use with Entity Framework.
    /// </summary>
    public class EntityFrameworkPayloadMaterializer<T> : IPayloadMaterializer<T> where T : class
    {
        private readonly DbContext _dbContext;
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly IQueryableResourceCollectionPayloadBuilder _queryableResourceCollectionPayloadBuilder;
        private readonly ISingleResourcePayloadBuilder _singleResourcePayloadBuilder;
        private readonly IEntityFrameworkResourceObjectMaterializer _entityFrameworkResourceObjectMaterializer;
        private readonly IBaseUrlService _baseUrlService;
        private readonly MethodInfo _getRelatedToManyMethod;
        private readonly MethodInfo _getRelatedToOneMethod;

        /// <summary>
        /// Creates a new EntityFrameworkPayloadMaterializer
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="queryableResourceCollectionPayloadBuilder"></param>
        /// <param name="singleResourcePayloadBuilder"></param>
        /// <param name="entityFrameworkResourceObjectMaterializer"></param>
        /// <param name="baseUrlService"></param>
        public EntityFrameworkPayloadMaterializer(
            DbContext dbContext,
            IResourceTypeRegistry resourceTypeRegistry,
            IQueryableResourceCollectionPayloadBuilder queryableResourceCollectionPayloadBuilder,
            ISingleResourcePayloadBuilder singleResourcePayloadBuilder,
            IEntityFrameworkResourceObjectMaterializer entityFrameworkResourceObjectMaterializer,
            IBaseUrlService baseUrlService)
        {
            _dbContext = dbContext;
            _resourceTypeRegistry = resourceTypeRegistry;
            _queryableResourceCollectionPayloadBuilder = queryableResourceCollectionPayloadBuilder;
            _singleResourcePayloadBuilder = singleResourcePayloadBuilder;
            _entityFrameworkResourceObjectMaterializer = entityFrameworkResourceObjectMaterializer;
            _baseUrlService = baseUrlService;
            _getRelatedToManyMethod = GetType()
                .GetMethod("GetRelatedToMany", BindingFlags.NonPublic | BindingFlags.Instance);
            _getRelatedToOneMethod = GetType()
                .GetMethod("GetRelatedToOne", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public virtual Task<IResourceCollectionPayload> GetRecords(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<T>().AsQueryable();
            return _queryableResourceCollectionPayloadBuilder.BuildPayload(query, request, cancellationToken);
        }

        public virtual async Task<ISingleResourcePayload> GetRecordById(string id, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var registration = _resourceTypeRegistry.GetRegistrationForType(typeof(T));
            var singleResource = await FilterById<T>(id, registration).FirstOrDefaultAsync(cancellationToken);
            return _singleResourcePayloadBuilder.BuildPayload(singleResource, apiBaseUrl, null);
        }

        public virtual async Task<IJsonApiPayload> GetRelated(string id, string relationshipKey, HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var registration = _resourceTypeRegistry.GetRegistrationForType(typeof (T));
            var relationship = (ResourceTypeRelationship) registration.GetFieldByName(relationshipKey);

            if (relationship.IsToMany)
            {
                var method = _getRelatedToManyMethod.MakeGenericMethod(relationship.RelatedType);
                var result = (Task<IResourceCollectionPayload>)method.Invoke(this, new object[] { id, relationship, request, cancellationToken });
                return await result;
            }
            else
            {
                var method = _getRelatedToOneMethod.MakeGenericMethod(relationship.RelatedType);
                var result = (Task<ISingleResourcePayload>)method.Invoke(this, new object[] { id, relationship, request, cancellationToken });
                return await result;
            }
        }

        public virtual async Task<ISingleResourcePayload> CreateRecord(ISingleResourcePayload requestPayload,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var newRecord = await MaterializeAsync(requestPayload.PrimaryData, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            var returnPayload = _singleResourcePayloadBuilder.BuildPayload(newRecord, apiBaseUrl, null);

            return returnPayload;
        }

        public virtual async Task<ISingleResourcePayload> UpdateRecord(string id, ISingleResourcePayload requestPayload,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiBaseUrl = GetBaseUrlFromRequest(request);
            var newRecord = await MaterializeAsync(requestPayload.PrimaryData, cancellationToken);
            var returnPayload = _singleResourcePayloadBuilder.BuildPayload(newRecord, apiBaseUrl, null);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return returnPayload;
        }

        public virtual async Task<IJsonApiPayload> DeleteRecord(string id, CancellationToken cancellationToken)
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
        protected async Task<IResourceCollectionPayload> GetRelatedToMany<TRelated>(string id,
            ResourceTypeRelationship relationship, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var primaryEntityRegistration = _resourceTypeRegistry.GetRegistrationForType(typeof (T));
            var param = Expression.Parameter(typeof(T));
            var accessorExpr = Expression.Property(param, relationship.Property);
            var lambda = Expression.Lambda<Func<T, IEnumerable<TRelated>>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<T>(id, primaryEntityRegistration);
            var relatedResourceQuery = primaryEntityQuery.SelectMany(lambda);

            return await _queryableResourceCollectionPayloadBuilder.BuildPayload(relatedResourceQuery, request, cancellationToken);
        }

        /// <summary>
        /// Generic method for getting the related resources for a to-one relationship
        /// </summary>
        protected async Task<ISingleResourcePayload> GetRelatedToOne<TRelated>(string id,
            ResourceTypeRelationship relationship, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var primaryEntityRegistration = _resourceTypeRegistry.GetRegistrationForType(typeof(T));
            var param = Expression.Parameter(typeof(T));
            var accessorExpr = Expression.Property(param, relationship.Property);
            var lambda = Expression.Lambda<Func<T, TRelated>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<T>(id, primaryEntityRegistration);
            var relatedResource = await primaryEntityQuery.Select(lambda).FirstOrDefaultAsync(cancellationToken);
            return _singleResourcePayloadBuilder.BuildPayload(relatedResource, GetBaseUrlFromRequest(request), null);
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
