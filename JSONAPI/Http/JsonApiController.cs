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
        /// Gets a single records matching the ID.
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
        /// Creates a new records corresponding to the data in the request payload.
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
        /// A no-op method. This should be overriden in subclasses if Delete is to be supported.
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
