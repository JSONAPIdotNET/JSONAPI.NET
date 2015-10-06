using System.Net.Http;

namespace JSONAPI.Http
{
    /// <summary>
    /// Service to extract sort expressions from an HTTP request
    /// </summary>
    public interface ISortExpressionExtractor
    {
        /// <summary>
        /// Extracts sort expressions from the request
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        string[] ExtractSortExpressions(HttpRequestMessage requestMessage);
    }
}
