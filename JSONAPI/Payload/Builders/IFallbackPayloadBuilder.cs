using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Service to create a payload when the type is unknown at compile-time
    /// </summary>
    public interface IFallbackPayloadBuilder
    {
        /// <summary>
        /// Builds a JSON API payload based on the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="requestMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="JsonApiException">Thrown when an error occurs when building the payload</exception>
        Task<IJsonApiPayload> BuildPayload(object obj, HttpRequestMessage requestMessage, CancellationToken cancellationToken);
    }
}