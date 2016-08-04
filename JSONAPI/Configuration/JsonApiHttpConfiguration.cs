using System;
using System.Web.Http;
using JSONAPI.ActionFilters;
using JSONAPI.Json;

namespace JSONAPI.Configuration
{
    /// <summary>
    /// Configures an HttpConfiguration object for use with JSONAPI.NET
    /// </summary>
    public class JsonApiHttpConfiguration
    {
        private readonly JsonApiFormatter _formatter;
        private readonly FallbackDocumentBuilderAttribute _fallbackDocumentBuilderAttribute;
        private readonly JsonApiExceptionFilterAttribute _jsonApiExceptionFilterAttribute;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiHttpConfiguration(JsonApiFormatter formatter,
            FallbackDocumentBuilderAttribute fallbackDocumentBuilderAttribute,
            JsonApiExceptionFilterAttribute jsonApiExceptionFilterAttribute)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            if (fallbackDocumentBuilderAttribute == null) throw new ArgumentNullException("fallbackDocumentBuilderAttribute");
            if (jsonApiExceptionFilterAttribute == null) throw new ArgumentNullException("jsonApiExceptionFilterAttribute");

            _formatter = formatter;
            _fallbackDocumentBuilderAttribute = fallbackDocumentBuilderAttribute;
            _jsonApiExceptionFilterAttribute = jsonApiExceptionFilterAttribute;
        }

        /// <summary>
        /// Applies the running configuration to an HttpConfiguration instance
        /// </summary>
        /// <param name="httpConfig">The HttpConfiguration to apply this JsonApiHttpConfiguration to</param>
        /// <param name="jsonApiConfiguration">configuration holding BaseUrlService wich could provide a context path.</param>
        public void Apply(HttpConfiguration httpConfig, IJsonApiConfiguration jsonApiConfiguration)
        {
            httpConfig.Formatters.Clear();
            httpConfig.Formatters.Add(_formatter);

            httpConfig.Filters.Add(_fallbackDocumentBuilderAttribute);
            httpConfig.Filters.Add(_jsonApiExceptionFilterAttribute);

            var contextPath = jsonApiConfiguration.CustomBaseUrlService?.GetContextPath();
            if (contextPath != null && !contextPath.Equals(string.Empty))
            {
                contextPath += "/";
            }

            // Web API routes
            httpConfig.Routes.MapHttpRoute("ResourceCollection", contextPath + "{resourceType}", new { controller = "Main" });
            httpConfig.Routes.MapHttpRoute("Resource", contextPath + "{resourceType}/{id}", new { controller = "Main" });
            httpConfig.Routes.MapHttpRoute("RelatedResource", contextPath + "{resourceType}/{id}/{relationshipName}", new { controller = "Main" });
        }
    }
}
