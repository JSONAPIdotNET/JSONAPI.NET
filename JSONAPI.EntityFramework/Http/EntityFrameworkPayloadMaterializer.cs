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
        private readonly MethodInfo _getRelatedToManyMethod;
        private readonly MethodInfo _getRelatedToOneMethod;

        /// <summary>
        /// Creates a new EntityFrameworkPayloadMaterializer
        /// </summary>
        /// <param name="dbContext"></param>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="queryableResourceCollectionPayloadBuilder"></param>
        /// <param name="singleResourcePayloadBuilder"></param>
        public EntityFrameworkPayloadMaterializer(
            DbContext dbContext,
            IResourceTypeRegistry resourceTypeRegistry,
            IQueryableResourceCollectionPayloadBuilder queryableResourceCollectionPayloadBuilder,
            ISingleResourcePayloadBuilder singleResourcePayloadBuilder)
        {
            _dbContext = dbContext;
            _resourceTypeRegistry = resourceTypeRegistry;
            _queryableResourceCollectionPayloadBuilder = queryableResourceCollectionPayloadBuilder;
            _singleResourcePayloadBuilder = singleResourcePayloadBuilder;
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
            var singleResource = await _dbContext.Set<T>().FindAsync(cancellationToken, id);
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
            var returnPayload = _singleResourcePayloadBuilder.BuildPayload(newRecord, apiBaseUrl, null);
            await _dbContext.SaveChangesAsync(cancellationToken);

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
        /// <param name="request"></param>
        /// <returns></returns>
        protected static string GetBaseUrlFromRequest(HttpRequestMessage request)
        {
            return new Uri(request.RequestUri.AbsoluteUri.Replace(request.RequestUri.PathAndQuery, String.Empty)).ToString();
        }

        /// <summary>
        /// Convert a resource object into a material record managed by EntityFramework.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<object> MaterializeAsync(IResourceObject resourceObject, CancellationToken cancellationToken)
        {
            var materializer = new EntityFrameworkResourceObjectMaterializer(_dbContext, _resourceTypeRegistry);
            return await materializer.MaterializeResourceObject(resourceObject, cancellationToken);
        }

        // ReSharper disable once UnusedMember.Local
        private async Task<IResourceCollectionPayload> GetRelatedToMany<TRelated>(string id,
            ResourceTypeRelationship relationship, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var param = Expression.Parameter(typeof(T));
            var accessorExpr = Expression.Property(param, relationship.Property);
            var lambda = Expression.Lambda<Func<T, IEnumerable<TRelated>>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<T>(id);
            var relatedResourceQuery = primaryEntityQuery.SelectMany(lambda);

            return await _queryableResourceCollectionPayloadBuilder.BuildPayload(relatedResourceQuery, request, cancellationToken);
        }

        // ReSharper disable once UnusedMember.Local
        private async Task<ISingleResourcePayload> GetRelatedToOne<TRelated>(string id,
            ResourceTypeRelationship relationship, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var param = Expression.Parameter(typeof(T));
            var accessorExpr = Expression.Property(param, relationship.Property);
            var lambda = Expression.Lambda<Func<T, TRelated>>(accessorExpr, param);

            var primaryEntityQuery = FilterById<T>(id);
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

        private IQueryable<TResource> FilterById<TResource>(string id, params Expression<Func<TResource, object>>[] includes) where TResource : class
        {
            var keyProp = GetKeyProp(typeof(TResource));
            var param = Expression.Parameter(typeof(TResource));
            var pkPropExpr = Expression.Property(param, keyProp);
            var idExpr = Expression.Constant(id);
            var equalsExpr = Expression.Equal(pkPropExpr, idExpr);
            var predicate = Expression.Lambda<Func<TResource, bool>>(equalsExpr, param);
            return Filter(predicate, includes);
        }

        private PropertyInfo GetKeyProp(Type t)
        {
            IEnumerable<string> propertyNames = null;
            while (t != null && t != typeof(Object))
            {
                var openMethod = typeof(DbContextExtensions).GetMethod("GetKeyNamesFromGeneric",
                    BindingFlags.Public | BindingFlags.Static);
                var method = openMethod.MakeGenericMethod(t);
                try
                {
                    propertyNames = (IEnumerable<string>)method.Invoke(null, new object[] { _dbContext });
                    break;
                }
                catch (TargetInvocationException)
                {
                    t = t.BaseType;
                }
            }

            if (propertyNames == null)
                throw new Exception(String.Format("Unable to detect key property for type {0}.", t.Name));

            return typeof(T).GetProperty(propertyNames.First());
        }
    }
}
