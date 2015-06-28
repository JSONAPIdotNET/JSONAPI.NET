using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using JSONAPI.Json;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Converts ObjectContent to payload form if it isn't already
    /// </summary>
    public class FallbackPayloadBuilderAttribute : ActionFilterAttribute
    {
        private readonly IFallbackPayloadBuilder _fallbackPayloadBuilder;
        private readonly IErrorPayloadBuilder _errorPayloadBuilder;

        /// <summary>
        /// Creates a FallbackPayloadBuilderAttribute
        /// </summary>
        /// <param name="fallbackPayloadBuilder"></param>
        /// <param name="errorPayloadBuilder"></param>
        public FallbackPayloadBuilderAttribute(IFallbackPayloadBuilder fallbackPayloadBuilder, IErrorPayloadBuilder errorPayloadBuilder)
        {
            _fallbackPayloadBuilder = fallbackPayloadBuilder;
            _errorPayloadBuilder = errorPayloadBuilder;
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext,
            CancellationToken cancellationToken)
        {
            var response = actionExecutedContext.Response;
            if (response != null && actionExecutedContext.Exception == null)
            {
                var content = actionExecutedContext.Response.Content;
                var objectContent = content as ObjectContent;
                if (content != null && objectContent == null)
                    return;

                if (objectContent != null)
                {
                    if (objectContent.Value == null)
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                            response.StatusCode = HttpStatusCode.NoContent;
                        return;
                    }

                    // These payload types should be passed through; they are already ready to be serialized.
                    if (objectContent.Value is ISingleResourcePayload ||
                        objectContent.Value is IResourceCollectionPayload)
                        return;

                    var errorPayload = objectContent.Value as IErrorPayload;
                    if (errorPayload != null)
                    {
                        actionExecutedContext.Response.StatusCode =
                            errorPayload.Errors.First().Status;
                        return;
                    }

                    object payloadValue;
                    var httpError = objectContent.Value as HttpError;
                    if (httpError != null)
                    {
                        payloadValue = _errorPayloadBuilder.BuildFromHttpError(httpError, actionExecutedContext.Response.StatusCode);
                    }
                    else
                    {
                        payloadValue =
                            await _fallbackPayloadBuilder.BuildPayload(objectContent.Value, actionExecutedContext.Request, cancellationToken);
                    }

                    errorPayload = payloadValue as IErrorPayload;
                    if (payloadValue is IErrorPayload)
                    {
                        actionExecutedContext.Response.StatusCode = errorPayload.Errors.First().Status;
                    }

                    actionExecutedContext.Response.Content = new ObjectContent(payloadValue.GetType(), payloadValue, objectContent.Formatter);
                }
            }

            await Task.Yield();
        }
    }
}
