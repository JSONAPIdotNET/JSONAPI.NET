using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Helper;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.QueryableResolvers;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp
{
    public class CommentSearchResourceQueryResolver : IResourceCollectionResolver<CommentSearch>
    {
        public Task<IQueryable<CommentSearch>> GetQueryForResourceCollection(IQueryable<CommentSearch> queryable, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var queryPairs = request.GetQueryNameValuePairs();
            foreach (var queryPair in queryPairs)
            {
                if (String.IsNullOrWhiteSpace(queryPair.Key))
                    continue;

                if (!queryPair.Key.StartsWith("searchterm"))
                    continue;

                var searchTerm = queryPair.Value;
                var predicate = PredicateBuilder.False<CommentSearch>();

                foreach (var str in Regex.Split(searchTerm, "\\s+"))
                {
                    predicate = predicate.Or(y => y.Text.Contains(str));
                }
                queryable= queryable.Where(predicate);
            }
            return Task.FromResult(queryable);
        }
    }
}