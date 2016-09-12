using System.Linq;
using System.Net.Http;

namespace JSONAPI.Http
{
    /// <summary>
    /// Default implementation of <see cref="IIncludeExpressionExtractor" />
    /// </summary>
    public class DefaultIncludeExpressionExtractor: IIncludeExpressionExtractor
    {
        private const string IncludeQueryParamKey = "include";

        public string[] ExtractIncludeExpressions(HttpRequestMessage requestMessage)
        {
            var queryParams = requestMessage.GetQueryNameValuePairs();
            var includeParam = queryParams.FirstOrDefault(kvp => kvp.Key == IncludeQueryParamKey);
            if (includeParam.Key != IncludeQueryParamKey) return new string[] { };
            return includeParam.Value.Split(',');
        }
    }
}
