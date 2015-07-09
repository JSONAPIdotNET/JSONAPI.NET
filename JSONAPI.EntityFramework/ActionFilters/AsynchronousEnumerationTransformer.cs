using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.QueryableTransformers;

namespace JSONAPI.EntityFramework.ActionFilters
{
    /// <summary>
    /// Enumerates an IQueryable asynchronously using Entity Framework's ToArrayAsync() method.
    /// </summary>
    public class AsynchronousEnumerationTransformer : IQueryableEnumerationTransformer
    {
        public async Task<T[]> Enumerate<T>(IQueryable<T> query, CancellationToken cancellationToken)
        {
            return await query.ToArrayAsync(cancellationToken);
        }

        public async Task<T> FirstOrDefault<T>(IQueryable<T> query, CancellationToken cancellationToken)
        {
            return await query.FirstOrDefaultAsync(cancellationToken);
        }
    }
}
