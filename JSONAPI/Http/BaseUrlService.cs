using System;
using System.Net.Http;

namespace JSONAPI.Http
{
    /// <summary>
    /// Default implementation of IBaseUrlService
    /// </summary>
    public class BaseUrlService : IBaseUrlService
    {
        public virtual string GetBaseUrl(HttpRequestMessage requestMessage)
        {
            return
                new Uri(requestMessage.RequestUri.AbsoluteUri.Replace(requestMessage.RequestUri.PathAndQuery,
                    String.Empty)).ToString();
        }
    }
}