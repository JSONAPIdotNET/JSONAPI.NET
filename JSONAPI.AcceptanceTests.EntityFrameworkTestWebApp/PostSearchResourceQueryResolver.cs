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
    public class PostSearchResourceQueryResolver: IResourceCollectionResolver<PostSearch>
    {
        public Task<IQueryable<PostSearch>> GetQueryForResourceCollection(IQueryable<PostSearch> queryable, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var queryPairs = request.GetQueryNameValuePairs();
            foreach (var queryPair in queryPairs)
            {
                if (String.IsNullOrWhiteSpace(queryPair.Key))
                    continue;

                if (!queryPair.Key.StartsWith("searchterm"))
                    continue;

                var searchTerm = queryPair.Value;
                var predicate = PredicateBuilder.False<PostSearch>();

                foreach (var str in Regex.Split(searchTerm, "\\s+"))
                {
                    predicate = predicate.Or(x => x.Title.Contains(str));
                    predicate = predicate.Or(x => x.Content.ToString().Contains(str));
                    predicate = predicate.Or(x => x.Comments.Any(y => y.Text.Contains(str)));
                }
                queryable= queryable.Where(predicate);
            }
            return Task.FromResult(queryable);
        }
    }
}