using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Documents;

namespace JSONAPI.Http
{
    /// <summary>
    /// Crafts a document corresponding to a related resource URL
    /// </summary>
    public interface IRelatedResourceDocumentMaterializer
    {

        /// <summary>
        /// Holds the current users principal and will be set by <see cref="JsonApiController"/> after locating the materializer.
        /// </summary>
        /// <returns></returns>
        IPrincipal Principal { get; set; }

        /// <summary>
        /// Builds a document containing the results of the relationship.
        /// </summary>
        Task<IJsonApiDocument> GetRelatedResourceDocument(string primaryResourceId, HttpRequestMessage request, CancellationToken cancellationToken);
    }
}