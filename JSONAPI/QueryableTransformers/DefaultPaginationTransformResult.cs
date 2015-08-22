using System.Linq;
using JSONAPI.ActionFilters;

namespace JSONAPI.QueryableTransformers
{
    /// <summary>
    /// Default implementation of IPaginationTransformResult`1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DefaultPaginationTransformResult<T> : IPaginationTransformResult<T>
    {
        public IQueryable<T> PagedQuery { get; set; }
        public bool PaginationWasApplied { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
}
