using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Documents;

namespace JSONAPI.Http
{
    /// <summary>
    /// This ApiController is capable of serving all requests using JSON API.
    /// </summary>
    public class JsonApiController : ApiController
    {
        private readonly IDocumentMaterializerLocator _documentMaterializerLocator;

        /// <summary>
        /// Creates a new JsonApiController
        /// </summary>
        /// <param name="documentMaterializerLocator">The service locator to get document materializers for a given resource type.</param>
        public JsonApiController(IDocumentMaterializerLocator documentMaterializerLocator)
        {
            _documentMaterializerLocator = documentMaterializerLocator;
        }

        /// <summary>
        /// Returns a document corresponding to a set of records of this type.
        /// </summary>
        public virtual async Task<IHttpActionResult> GetResourceCollection(string resourceType, CancellationToken cancellationToken)
        {
            var materializer = _documentMaterializerLocator.GetMaterializerByResourceTypeName(resourceType);
            var document = await materializer.GetRecords(Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Returns a document corresponding to the single record matching the ID.
        /// </summary>
        public virtual async Task<IHttpActionResult> Get(string resourceType, string id, CancellationToken cancellationToken)
        {
            var materializer = _documentMaterializerLocator.GetMaterializerByResourceTypeName(resourceType);
            var document = await materializer.GetRecordById(id, Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Returns a document corresponding to the resource(s) related to the resource identified by the ID,
        /// and the relationship name.
        /// </summary>
        public virtual async Task<IHttpActionResult> GetRelatedResource(string resourceType, string id, string relationshipName, CancellationToken cancellationToken)
        {
            var materializer = _documentMaterializerLocator.GetRelatedResourceMaterializer(resourceType, relationshipName);
            var document = await materializer.GetRelatedResourceDocument(id, Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Creates a new record corresponding to the data in the request document.
        /// </summary>
        public virtual async Task<IHttpActionResult> Post(string resourceType, [FromBody]ISingleResourceDocument requestDocument, CancellationToken cancellationToken)
        {
            var materializer = _documentMaterializerLocator.GetMaterializerByResourceTypeName(resourceType);
            var document = await materializer.CreateRecord(requestDocument, Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Updates the record with the given ID with data from the request payloaad.
        /// </summary>
        public virtual async Task<IHttpActionResult> Patch(string resourceType, string id, [FromBody]ISingleResourceDocument requestDocument, CancellationToken cancellationToken)
        {
            var materializer = _documentMaterializerLocator.GetMaterializerByResourceTypeName(resourceType);
            var document = await materializer.UpdateRecord(id, requestDocument, Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Deletes the record corresponding to the ID.
        /// </summary>
        public virtual async Task<IHttpActionResult> Delete(string resourceType, string id, CancellationToken cancellationToken)
        {
            var materializer = _documentMaterializerLocator.GetMaterializerByResourceTypeName(resourceType);
            var document = await materializer.DeleteRecord(id, cancellationToken);
            return Ok(document);
        }
    }
}
