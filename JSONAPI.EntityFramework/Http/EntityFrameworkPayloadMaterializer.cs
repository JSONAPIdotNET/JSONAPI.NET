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
    /// Implementation of <see cref="IPayloadMaterializer"/> for use with Entity Framework.
    /// </summary>
    public class EntityFrameworkPayloadMaterializer : IPayloadMaterializer
    {
        private readonly string _apiBaseUrl;
        private readonly DbContext _dbContext;
        private readonly IResourceTypeRegistry _resourceTypeRegistry;
        private readonly IQueryableResourceCollectionPayloadBuilder _queryableResourceCollectionPayloadBuilder;
        private readonly ISingleResourcePayloadBuilder _singleResourcePayloadBuilder;

        /// <summary>
        /// Creates a new EntityFrameworkPayloadMaterializer
        /// </summary>
        /// <param name="apiBaseUrl">The base url of the API, e.g. https://www.example.com</param>
        /// <param name="dbContext"></param>
        /// <param name="resourceTypeRegistry"></param>
        /// <param name="queryableResourceCollectionPayloadBuilder"></param>
        /// <param name="singleResourcePayloadBuilder"></param>
        public EntityFrameworkPayloadMaterializer(
            string apiBaseUrl,
            DbContext dbContext,
            IResourceTypeRegistry resourceTypeRegistry,
            IQueryableResourceCollectionPayloadBuilder queryableResourceCollectionPayloadBuilder,
            ISingleResourcePayloadBuilder singleResourcePayloadBuilder)
        {
            _apiBaseUrl = apiBaseUrl;
            _dbContext = dbContext;
            _resourceTypeRegistry = resourceTypeRegistry;
            _queryableResourceCollectionPayloadBuilder = queryableResourceCollectionPayloadBuilder;
            _singleResourcePayloadBuilder = singleResourcePayloadBuilder;
        }

        public Task<IResourceCollectionPayload> GetRecords<T>(HttpRequestMessage request, CancellationToken cancellationToken) where T : class
        {
            var query = _dbContext.Set<T>().AsQueryable();
            return _queryableResourceCollectionPayloadBuilder.BuildPayload(query, request, cancellationToken);
        }

        public async Task<ISingleResourcePayload> GetRecordById<T>(string id, HttpRequestMessage request, CancellationToken cancellationToken) where T : class
        {
            var singleResource = await _dbContext.Set<T>().FindAsync(cancellationToken, id);
            return _singleResourcePayloadBuilder.BuildPayload(singleResource, _apiBaseUrl, null);
        }

        public async Task<ISingleResourcePayload> CreateRecord<T>(ISingleResourcePayload requestPayload, HttpRequestMessage request, CancellationToken cancellationToken) where T : class
        {
            var newRecord = await MaterializeAsync(requestPayload.PrimaryData, cancellationToken);
            var returnPayload = _singleResourcePayloadBuilder.BuildPayload(newRecord, _apiBaseUrl, null);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return returnPayload;
        }

        public async Task<ISingleResourcePayload> UpdateRecord<T>(string id, ISingleResourcePayload requestPayload, HttpRequestMessage request, CancellationToken cancellationToken) where T : class
        {
            var newRecord = await MaterializeAsync(requestPayload.PrimaryData, cancellationToken);
            var returnPayload = _singleResourcePayloadBuilder.BuildPayload(newRecord, _apiBaseUrl, null);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return returnPayload;
        }

        public async Task<IJsonApiPayload> DeleteRecord<T>(string id, CancellationToken cancellationToken) where T : class
        {
            var singleResource = await _dbContext.Set<T>().FindAsync(cancellationToken, id);
            _dbContext.Set<T>().Remove(singleResource);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return null;
        }

        /// <summary>
        /// Convert a resource object into a material record managed by EntityFramework.
        /// </summary>
        /// <returns></returns>
        protected virtual async Task<object> MaterializeAsync(IResourceObject resourceObject, CancellationToken cancellationToken)
        {
            var materializer = new EntityFrameworkEntityFrameworkResourceObjectMaterializer(_dbContext, _resourceTypeRegistry);
            return await materializer.MaterializeResourceObject(resourceObject, cancellationToken);
        }
    }
}
