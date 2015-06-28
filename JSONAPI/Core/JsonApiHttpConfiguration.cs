using System;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using JSONAPI.ActionFilters;
using JSONAPI.Http;
using JSONAPI.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Configures an HttpConfiguration object for use with JSONAPI.NET
    /// </summary>
    public class JsonApiHttpConfiguration
    {
        private readonly JsonApiFormatter _formatter;
        private readonly FallbackPayloadBuilderAttribute _fallbackPayloadBuilderAttribute;
        private readonly JsonApiExceptionFilterAttribute _jsonApiExceptionFilterAttribute;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiHttpConfiguration(JsonApiFormatter formatter,
            FallbackPayloadBuilderAttribute fallbackPayloadBuilderAttribute,
            JsonApiExceptionFilterAttribute jsonApiExceptionFilterAttribute)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            if (fallbackPayloadBuilderAttribute == null) throw new ArgumentNullException("fallbackPayloadBuilderAttribute");
            if (jsonApiExceptionFilterAttribute == null) throw new ArgumentNullException("jsonApiExceptionFilterAttribute");

            _formatter = formatter;
            _fallbackPayloadBuilderAttribute = fallbackPayloadBuilderAttribute;
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

            httpConfig.Filters.Add(_fallbackPayloadBuilderAttribute);
            httpConfig.Filters.Add(_jsonApiExceptionFilterAttribute);

            httpConfig.Services.Replace(typeof(IHttpControllerSelector),
                new PascalizedControllerSelector(httpConfig));
        }
    }
}
