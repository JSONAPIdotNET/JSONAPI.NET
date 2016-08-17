using System.Net.Http;

namespace JSONAPI.Http
{
    /// <summary>
    /// Service to extract include expressions from an HTTP request
    /// </summary>
    public interface IIncludeExpressionExtractor
    {
        /// <summary>
        /// Extracts include expressions from the request
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        string[] ExtractIncludeExpressions(HttpRequestMessage requestMessage);
    }
}
