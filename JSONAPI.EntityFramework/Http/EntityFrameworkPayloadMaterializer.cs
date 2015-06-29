using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
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
    }
}
