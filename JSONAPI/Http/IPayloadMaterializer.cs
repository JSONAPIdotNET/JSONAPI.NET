using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Payload;

namespace JSONAPI.Http
{
    /// <summary>
    /// This service provides the glue between JSONAPI.NET and your persistence layer.
    /// </summary>
    public interface IPayloadMaterializer<T> where T : class
    {
        /// <summary>
        /// Returns a payload containing records that are filtered, sorted,
        /// and paginated according to query parameters present in the provided request.
        /// </summary>
        /// <returns></returns>
        Task<IResourceCollectionPayload> GetRecords(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a payload with the resource identified by the given ID.
        /// </summary>
        /// <returns></returns>
        Task<ISingleResourcePayload> GetRecordById(string id, HttpRequestMessage request,
            CancellationToken cancellationToken);

        /// <summary>
        /// Creates a record corresponding to the data in the request payload, and returns a payload
        /// corresponding to the created record.
        /// </summary>
        /// <returns></returns>
        Task<ISingleResourcePayload> CreateRecord(ISingleResourcePayload requestPayload, HttpRequestMessage request,
            CancellationToken cancellationToken);

        /// <summary>
        /// Updates the record corresponding to the data in the request payload, and returns a payload
        /// corresponding to the updated record.
        /// </summary>
        /// <returns></returns>
        Task<ISingleResourcePayload> UpdateRecord(string id, ISingleResourcePayload requestPayload,
            HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the record corresponding to the given id.
        /// </summary>
        /// <returns></returns>
        Task<IJsonApiPayload> DeleteRecord(string id, CancellationToken cancellationToken);
    }
}