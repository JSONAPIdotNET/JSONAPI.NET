using System.Net.Http;
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
        /// Builds a document containing the results of the relationship.
        /// </summary>
        Task<IJsonApiDocument> GetRelatedResourceDocument(string primaryResourceId, HttpRequestMessage request, CancellationToken cancellationToken);
    }
}