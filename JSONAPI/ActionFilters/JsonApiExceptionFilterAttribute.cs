using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using JSONAPI.Json;
using JSONAPI.Payload.Builders;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Filter for catching exceptions and converting them to IErrorPayload
    /// </summary>
    public class JsonApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IErrorPayloadBuilder _errorPayloadBuilder;
        private readonly JsonApiFormatter _jsonApiFormatter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorPayloadBuilder"></param>
        /// <param name="jsonApiFormatter"></param>
        public JsonApiExceptionFilterAttribute(IErrorPayloadBuilder errorPayloadBuilder, JsonApiFormatter jsonApiFormatter)
        {
            _errorPayloadBuilder = errorPayloadBuilder;
            _jsonApiFormatter = jsonApiFormatter;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var payload = _errorPayloadBuilder.BuildFromException(actionExecutedContext.Exception);
            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new ObjectContent(payload.GetType(), payload, _jsonApiFormatter)
            };
            if (payload.Errors != null && payload.Errors.Length > 0)
            {
                var status = payload.Errors.First().Status;
                actionExecutedContext.Response.StatusCode = status != default(HttpStatusCode) ? status : HttpStatusCode.InternalServerError;
            }
        }
    }
}
