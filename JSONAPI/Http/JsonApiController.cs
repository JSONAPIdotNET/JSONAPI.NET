using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Documents;

namespace JSONAPI.Http
{
    /// <summary>
    /// This generic ApiController provides JSON API-compatible endpoints corresponding to a
    /// registered type.
    /// </summary>
    public class JsonApiController<T> : ApiController where T : class
    {
        private readonly IDocumentMaterializer<T> _documentMaterializer;

        /// <summary>
        /// Creates a new ApiController
        /// </summary>
        /// <param name="documentMaterializer"></param>
        public JsonApiController(IDocumentMaterializer<T> documentMaterializer)
        {
            _documentMaterializer = documentMaterializer;
        }

        /// <summary>
        /// Returns a document corresponding to a set of records of this type.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Get(CancellationToken cancellationToken)
        {
            var document = await _documentMaterializer.GetRecords(Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Returns a document corresponding to the single record matching the ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var document = await _documentMaterializer.GetRecordById(id, Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Returns a document corresponding to the resource(s) related to the resource identified by the ID,
        /// and the relationship name.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="relationshipName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> GetRelatedResource(string id, string relationshipName, CancellationToken cancellationToken)
        {
            var document = await _documentMaterializer.GetRelated(id, relationshipName, Request, cancellationToken);
            return Ok(document);
        }


        /// <summary>
        /// Creates a new record corresponding to the data in the request document.
        /// </summary>
        /// <param name="requestDocument"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Post([FromBody]ISingleResourceDocument requestDocument, CancellationToken cancellationToken)
        {
            var document = await _documentMaterializer.CreateRecord(requestDocument, Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Updates the record with the given ID with data from the request payloaad.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestDocument"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Patch(string id, [FromBody]ISingleResourceDocument requestDocument, CancellationToken cancellationToken)
        {
            var document = await _documentMaterializer.UpdateRecord(id, requestDocument, Request, cancellationToken);
            return Ok(document);
        }

        /// <summary>
        /// Deletes the record corresponding to the ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        public virtual async Task<IHttpActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var document = await _documentMaterializer.DeleteRecord(id, cancellationToken);
            return Ok(document);
        }
    }
}
