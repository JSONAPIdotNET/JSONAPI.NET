using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Converts ObjectContent to JSON API document form if it isn't already
    /// </summary>
    public class FallbackDocumentBuilderAttribute : ActionFilterAttribute
    {
        private readonly IFallbackDocumentBuilder _fallbackDocumentBuilder;
        private readonly IErrorDocumentBuilder _errorDocumentBuilder;

        /// <summary>
        /// Creates a FallbackDocumentBuilderAttribute
        /// </summary>
        /// <param name="fallbackDocumentBuilder"></param>
        /// <param name="errorDocumentBuilder"></param>
        public FallbackDocumentBuilderAttribute(IFallbackDocumentBuilder fallbackDocumentBuilder, IErrorDocumentBuilder errorDocumentBuilder)
        {
            _fallbackDocumentBuilder = fallbackDocumentBuilder;
            _errorDocumentBuilder = errorDocumentBuilder;
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

                    // These document types should be passed through; they are already ready to be serialized.
                    if (objectContent.Value is ISingleResourceDocument ||
                        objectContent.Value is IResourceCollectionDocument)
                        return;

                    var errorDocument = objectContent.Value as IErrorDocument;
                    if (errorDocument != null)
                    {
                        actionExecutedContext.Response.StatusCode =
                            errorDocument.Errors.First().Status;
                        return;
                    }

                    object documentValue;
                    var httpError = objectContent.Value as HttpError;
                    if (httpError != null)
                    {
                        documentValue = _errorDocumentBuilder.BuildFromHttpError(httpError, actionExecutedContext.Response.StatusCode);
                    }
                    else
                    {
                        documentValue =
                            await _fallbackDocumentBuilder.BuildDocument(objectContent.Value, actionExecutedContext.Request, cancellationToken);
                    }

                    errorDocument = documentValue as IErrorDocument;
                    if (documentValue is IErrorDocument)
                    {
                        actionExecutedContext.Response.StatusCode = errorDocument.Errors.First().Status;
                    }

                    actionExecutedContext.Response.Content = new ObjectContent(documentValue.GetType(), documentValue, objectContent.Formatter);
                }
            }

            await Task.Yield();
        }
    }
}
