using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JSONAPI.QueryableResolvers
{
    public interface IResourceCollectionResolver<TResource>
    {
        Task<IQueryable<TResource>> GetQueryForResourceCollection(IQueryable<TResource> queryable, HttpRequestMessage request, CancellationToken cancellationToken);
    }
}
