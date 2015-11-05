using System.Linq;
using System.Net.Http;

namespace JSONAPI.Http
{
    /// <summary>
    /// Default implementation of <see cref="ISortExpressionExtractor" />
    /// </summary>
    public class DefaultSortExpressionExtractor : ISortExpressionExtractor
    {
        private const string SortQueryParamKey = "sort";

        public string[] ExtractSortExpressions(HttpRequestMessage requestMessage)
        {
            var queryParams = requestMessage.GetQueryNameValuePairs();
            var sortParam = queryParams.FirstOrDefault(kvp => kvp.Key == SortQueryParamKey);
            if (sortParam.Key != SortQueryParamKey) return new string[] {};
            return sortParam.Value.Split(',');
        }
    }
}