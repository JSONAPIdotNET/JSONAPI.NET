using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.Documents;

namespace JSONAPI.Http
{
    /// <summary>
    /// This service provides the glue between JSONAPI.NET and your persistence layer.
    /// </summary>
    public interface IDocumentMaterializer
    {
        /// <summary>
        /// Returns a document containing records that are filtered, sorted,
        /// and paginated according to query parameters present in the provided request.
        /// </summary>
        Task<IResourceCollectionDocument> GetRecords(HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Returns a document with the resource identified by the given ID.
        /// </summary>
        Task<ISingleResourceDocument> GetRecordById(string id, HttpRequestMessage request,
            CancellationToken cancellationToken);

        /// <summary>
        /// Creates a record corresponding to the data in the request document, and returns a document
        /// corresponding to the created record.
        /// </summary>
        Task<ISingleResourceDocument> CreateRecord(ISingleResourceDocument requestDocument, HttpRequestMessage request,
            CancellationToken cancellationToken);

        /// <summary>
        /// Updates the record corresponding to the data in the request document, and returns a document
        /// corresponding to the updated record.
        /// </summary>
        Task<ISingleResourceDocument> UpdateRecord(string id, ISingleResourceDocument requestDocument,
            HttpRequestMessage request, CancellationToken cancellationToken);

        /// <summary>
        /// Deletes the record corresponding to the given id.
        /// </summary>
        Task<IJsonApiDocument> DeleteRecord(string id, CancellationToken cancellationToken);
    }
}