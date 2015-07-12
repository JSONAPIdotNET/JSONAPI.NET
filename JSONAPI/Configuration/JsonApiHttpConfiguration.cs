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
        public void Apply(HttpConfiguration httpConfig)
        {
            httpConfig.Formatters.Clear();
            httpConfig.Formatters.Add(_formatter);

            httpConfig.Filters.Add(_fallbackDocumentBuilderAttribute);
            httpConfig.Filters.Add(_jsonApiExceptionFilterAttribute);

            // Web API routes
            httpConfig.Routes.MapHttpRoute("ResourceCollection", "{resourceType}", new { controller = "Main" });
            httpConfig.Routes.MapHttpRoute("Resource", "{resourceType}/{id}", new { controller = "Main" });
            httpConfig.Routes.MapHttpRoute("RelatedResource", "{resourceType}/{id}/{relationshipName}", new { controller = "Main" });
        }
    }
}
