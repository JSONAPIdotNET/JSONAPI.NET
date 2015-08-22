using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using JSONAPI.Documents.Builders;
using JSONAPI.Json;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Filter for catching exceptions and converting them to IErrorDocument
    /// </summary>
    public class JsonApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private readonly IErrorDocumentBuilder _errorDocumentBuilder;
        private readonly JsonApiFormatter _jsonApiFormatter;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorDocumentBuilder"></param>
        /// <param name="jsonApiFormatter"></param>
        public JsonApiExceptionFilterAttribute(IErrorDocumentBuilder errorDocumentBuilder, JsonApiFormatter jsonApiFormatter)
        {
            _errorDocumentBuilder = errorDocumentBuilder;
            _jsonApiFormatter = jsonApiFormatter;
        }

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var document = _errorDocumentBuilder.BuildFromException(actionExecutedContext.Exception);
            actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new ObjectContent(document.GetType(), document, _jsonApiFormatter)
            };
            if (document.Errors != null && document.Errors.Length > 0)
            {
                var status = document.Errors.First().Status;
                actionExecutedContext.Response.StatusCode = status != default(HttpStatusCode) ? status : HttpStatusCode.InternalServerError;
            }
        }
    }
}
