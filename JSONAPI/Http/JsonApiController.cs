using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Payload;

namespace JSONAPI.Http
{
    /// <summary>
    /// This generic ApiController provides JSON API-compatible endpoints corresponding to a
    /// registered type.
    /// </summary>
    public class JsonApiController<T> : ApiController where T : class
    {
        private readonly IPayloadMaterializer<T> _payloadMaterializer;

        /// <summary>
        /// Creates a new ApiController
        /// </summary>
        /// <param name="payloadMaterializer"></param>
        public JsonApiController(IPayloadMaterializer<T> payloadMaterializer)
        {
            _payloadMaterializer = payloadMaterializer;
        }

        /// <summary>
        /// Returns a payload corresponding to a set of records of this type.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Get(CancellationToken cancellationToken)
        {
            var payload = await _payloadMaterializer.GetRecords(Request, cancellationToken);
            return Ok(payload);
        }

        /// <summary>
        /// Returns a payload corresponding to the single record matching the ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Get(string id, CancellationToken cancellationToken)
        {
            var payload = await _payloadMaterializer.GetRecordById(id, Request, cancellationToken);
            return Ok(payload);
        }

        /// <summary>
        /// Returns a payload corresponding to the resource(s) related to the resource identified by the ID,
        /// and the relationship name.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="relationshipName"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> GetRelatedResource(string id, string relationshipName, CancellationToken cancellationToken)
        {
            var payload = await _payloadMaterializer.GetRelated(id, relationshipName, Request, cancellationToken);
            return Ok(payload);
        }


        /// <summary>
        /// Creates a new record corresponding to the data in the request payload.
        /// </summary>
        /// <param name="requestPayload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Post([FromBody]ISingleResourcePayload requestPayload, CancellationToken cancellationToken)
        {
            var payload = await _payloadMaterializer.CreateRecord(requestPayload, Request, cancellationToken);
            return Ok(payload);
        }

        /// <summary>
        /// Updates the record with the given ID with data from the request payloaad.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="requestPayload"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public virtual async Task<IHttpActionResult> Patch(string id, [FromBody]ISingleResourcePayload requestPayload, CancellationToken cancellationToken)
        {
            var payload = await _payloadMaterializer.UpdateRecord(id, requestPayload, Request, cancellationToken);
            return Ok(payload);
        }

        /// <summary>
        /// Deletes the record corresponding to the ID.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        public virtual async Task<IHttpActionResult> Delete(string id, CancellationToken cancellationToken)
        {
            var payload = await _payloadMaterializer.DeleteRecord(id, cancellationToken);
            return Ok(payload);
        }
    }
}
