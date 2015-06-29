using System.Net.Http;

namespace JSONAPI.Http
{
    /// <summary>
    /// Service allowing you to get the base URL for a request
    /// </summary>
    public interface IBaseUrlService
    {
        /// <summary>
        /// Gets the base URL for a request
        /// </summary>
        string GetBaseUrl(HttpRequestMessage requestMessage);
    }
}
