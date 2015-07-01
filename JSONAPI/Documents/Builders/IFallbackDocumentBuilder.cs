using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Service to create a document when the type is unknown at compile-time
    /// </summary>
    public interface IFallbackDocumentBuilder
    {
        /// <summary>
        /// Builds a JSON API document based on the given object
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="requestMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="JsonApiException">Thrown when an error occurs when building the document</exception>
        Task<IJsonApiDocument> BuildDocument(object obj, HttpRequestMessage requestMessage, CancellationToken cancellationToken);
    }
}