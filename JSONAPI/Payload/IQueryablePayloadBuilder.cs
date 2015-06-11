using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.Payload
{
    /// <summary>
    /// This interface is responsible for building IPayload objects based on IQueryable ObjectContent
    /// </summary>
    public interface IQueryablePayloadBuilder
    {
        /// <summary>
        /// Builds a payload object for the given query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<IPayload> BuildPayload<T>(IQueryable<T> query, HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
