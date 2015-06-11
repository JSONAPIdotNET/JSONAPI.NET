using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Filters;
using JSONAPI.Payload;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// This is an action filter that you can insert into your Web API pipeline to perform various transforms
    /// on IQueryable payloads.
    /// </summary>
    public class JsonApiQueryableAttribute : ActionFilterAttribute
    {
        private readonly IQueryablePayloadBuilder _payloadBuilder;
        private readonly Lazy<MethodInfo> _openBuildPayloadMethod;

        /// <summary>
        /// Creates a new JsonApiQueryableAttribute.
        /// </summary>
        public JsonApiQueryableAttribute(IQueryablePayloadBuilder payloadBuilder)
        {
            _payloadBuilder = payloadBuilder;
            _openBuildPayloadMethod =
                new Lazy<MethodInfo>(() => _payloadBuilder.GetType().GetMethod("BuildPayload", BindingFlags.Instance | BindingFlags.Public));
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (actionExecutedContext.Response != null)
            {
                var objectContent = actionExecutedContext.Response.Content as ObjectContent;
                if (objectContent != null)
                {
                    var objectType = objectContent.ObjectType;
                    if (objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                    {
                        var queryableElementType = objectType.GenericTypeArguments[0];
                        var buildPayloadMethod = _openBuildPayloadMethod.Value.MakeGenericMethod(queryableElementType);

                        try
                        {
                            dynamic materializedQueryTask;
                            try
                            {
                                materializedQueryTask = buildPayloadMethod.Invoke(_payloadBuilder,
                                    new[] {objectContent.Value, actionExecutedContext.Request, cancellationToken});
                            }
                            catch (TargetInvocationException ex)
                            {
                                throw ex.InnerException;
                            }

                            var materializedResults = await materializedQueryTask;

                            actionExecutedContext.Response.Content = new ObjectContent(
                                materializedResults.GetType(),
                                materializedResults,
                                objectContent.Formatter);
                        }
                        catch (HttpResponseException)
                        {
                            throw;
                        }
                        catch (QueryableTransformException ex)
                        {
                            throw new HttpResponseException(
                                actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, ex.Message));
                        }
                        catch (Exception ex)
                        {
                            throw new HttpResponseException(
                                actionExecutedContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message));
                        }
                    }
                }
            }
        }
    }
}
