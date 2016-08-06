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
        private Uri _publicOrigin;

        /// <summary>
        /// Default constructor
        /// </summary>
        public BaseUrlService() { }

        /// <summary>
        /// Constructor which provides a context path for the routes of JSONAPI.NET
        /// </summary>
        /// <param name="contextPath">context path for the routes</param>
        public BaseUrlService(string contextPath)
        {
            CleanContextPath(contextPath);
        }

        /// <summary>
        /// Constructor which provides a public origin host and a context path for the routes of JSONAPI.NET.
        /// If only public origin is desired provide emtpy string to contextPath.
        /// </summary>
        /// <param name="publicOrigin">public hostname</param>
        /// <param name="contextPath">context path for the routes</param>
        public BaseUrlService(Uri publicOrigin, string contextPath)
        {
            CleanContextPath(contextPath);
            this._publicOrigin = publicOrigin;
        }

        /// <summary>
        /// Retrieve the base path to provide in responses.
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public virtual string GetBaseUrl(HttpRequestMessage requestMessage)
        {
            string pathAndQuery;
            string absolutUri = requestMessage.RequestUri.AbsoluteUri;
            if (_publicOrigin != null)
            {
                var publicUriBuilder = new UriBuilder(absolutUri)
                {
                    Host = _publicOrigin.Host,
                    Scheme = _publicOrigin.Scheme,
                    Port = _publicOrigin.Port
                };
                absolutUri = publicUriBuilder.Uri.AbsoluteUri;
                pathAndQuery = publicUriBuilder.Uri.PathAndQuery;
            }
            else
            {
                pathAndQuery = requestMessage.RequestUri.PathAndQuery;
            }
            pathAndQuery = RemoveFromBegin(pathAndQuery, GetContextPath());
            pathAndQuery= pathAndQuery.TrimStart('/');
            var baseUrl = RemoveFromEnd(absolutUri, pathAndQuery);
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