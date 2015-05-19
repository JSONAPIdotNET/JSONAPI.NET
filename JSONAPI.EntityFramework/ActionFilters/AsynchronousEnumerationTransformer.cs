using System;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.ActionFilters;

namespace JSONAPI.EntityFramework.ActionFilters
{
    /// <summary>
    /// Enumerates an IQueryable asynchronously using Entity Framework's ToArrayAsync() method.
    /// </summary>
    public class AsynchronousEnumerationTransformer : IQueryableEnumerationTransformer
    {
        private readonly Lazy<MethodInfo> _toArrayAsyncMethod = new Lazy<MethodInfo>(() =>
               typeof(QueryableExtensions).GetMethods().FirstOrDefault(x => x.Name == "ToArrayAsync" && x.GetParameters().Count() == 2));

        public async Task<T[]> Enumerate<T>(IQueryable<T> query, CancellationToken cancellationToken)
        {
            var queryableElementType = typeof (T);
            var openToArrayAsyncMethod = _toArrayAsyncMethod.Value;
            var toArrayAsyncMethod = openToArrayAsyncMethod.MakeGenericMethod(queryableElementType);
            var invocation = (dynamic)toArrayAsyncMethod.Invoke(null, new object[] { query, cancellationToken });

            var resultArray = await invocation;
            return resultArray;
        }
    }
}
