using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.ActionFilters
{
    /// <summary>
    /// Synchronously enumerates an IQueryable
    /// </summary>
    public class SynchronousEnumerationTransformer : IQueryableEnumerationTransformer
    {
        public Task<T[]> Enumerate<T>(IQueryable<T> query, CancellationToken cancellationToken)
        {
            return Task.FromResult(query.ToArray());
        }
    }
}
