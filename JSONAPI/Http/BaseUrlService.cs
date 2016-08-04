using System;
using System.Net.Http;

namespace JSONAPI.Http
{
    /// <summary>
    /// Default implementation of IBaseUrlService
    /// </summary>
    public class BaseUrlService : IBaseUrlService
    {
        private string _contextPath = string.Empty;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseUrlService() { }

        public BaseUrlService(string contextPath)
        {
            CleanContextPath(contextPath);
        }


        public virtual string GetBaseUrl(HttpRequestMessage requestMessage)
        {
            var pathAndQuery = requestMessage.RequestUri.PathAndQuery;
            pathAndQuery = RemoveFromBegin(pathAndQuery, GetContextPath());
            var baseUrl = RemoveFromEnd(requestMessage.RequestUri.AbsoluteUri, pathAndQuery);
            return baseUrl;
        }

        /// <summary>
        /// Provides the context path to serve JSONAPI.NET without leading and trailing slash.
        /// </summary>
        /// <returns></returns>
        public string GetContextPath()
        {
            return _contextPath;
        }

        /// <summary>
        ///  Makes sure thre are no slashes at the beginnig or end.
        /// </summary>
        /// <param name="contextPath"></param>
        private void CleanContextPath(string contextPath)
        {
            if (!string.IsNullOrEmpty(contextPath) && !contextPath.EndsWith("/"))
            {
                contextPath = contextPath.TrimEnd('/');
            }
            if (!string.IsNullOrEmpty(contextPath) && contextPath.StartsWith("/"))
            {
                contextPath = contextPath.TrimStart('/');
            }
            _contextPath = contextPath;
        }


        private string RemoveFromEnd(string input, string suffix)
        {
            if (input.EndsWith(suffix))
            {
                return input.Substring(0, input.Length - suffix.Length);
            }
            return input;
        }
        private string RemoveFromBegin(string input, string prefix)
        {
            prefix = "/" + prefix;
            if (input.StartsWith(prefix))
            {
                return input.Substring(prefix.Length, input.Length - prefix.Length);
            }
            return input;
        }
    }
}