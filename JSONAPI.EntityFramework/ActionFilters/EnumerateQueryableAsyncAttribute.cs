using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace JSONAPI.EntityFramework.ActionFilters
{
    public class EnumerateQueryableAsyncAttribute : ActionFilterAttribute
    {
        private readonly Lazy<MethodInfo> _toArrayAsyncMethod = new Lazy<MethodInfo>(() =>
               typeof(QueryableExtensions).GetMethods().FirstOrDefault(x => x.Name == "ToArrayAsync" && x.GetParameters().Count() == 2));

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
                        var openToArrayAsyncMethod = _toArrayAsyncMethod.Value;
                        var toArrayAsyncMethod = openToArrayAsyncMethod.MakeGenericMethod(queryableElementType);
                        var invocation = (dynamic)toArrayAsyncMethod.Invoke(null, new[] { objectContent.Value, cancellationToken });

                        var resultArray = await invocation;
                        actionExecutedContext.Response.Content = new ObjectContent(resultArray.GetType(), resultArray, objectContent.Formatter);
                    }
                }
            }
        }
    }
}
