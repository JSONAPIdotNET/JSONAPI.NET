using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using JSONAPI.ActionFilters;
using JSONAPI.Http;
using JSONAPI.Json;
using JSONAPI.Payload;

namespace JSONAPI.Core
{
    /// <summary>
    /// Configuration API for JSONAPI.NET
    /// </summary>
    public class JsonApiConfiguration
    {
        private readonly IModelManager _modelManager;
        private Func<IQueryablePayloadBuilder> _payloadBuilderFactory;

        /// <summary>
        /// Creates a new configuration
        /// </summary>
        public JsonApiConfiguration(IModelManager modelManager)
        {
            if (modelManager == null) throw new Exception("You must provide a model manager to begin configuration.");

            _modelManager = modelManager;
            _payloadBuilderFactory = () => new DefaultQueryablePayloadBuilderConfiguration().GetBuilder(modelManager);
        }

        /// <summary>
        /// Allows configuring the default queryable payload builder
        /// </summary>
        /// <param name="configurationAction">Provides access to a fluent DefaultQueryablePayloadBuilderConfiguration object</param>
        /// <returns>The same configuration object the method was called on.</returns>
        public JsonApiConfiguration UsingDefaultQueryablePayloadBuilder(Action<DefaultQueryablePayloadBuilderConfiguration> configurationAction)
        {
            _payloadBuilderFactory = () =>
            {
                var configuration = new DefaultQueryablePayloadBuilderConfiguration();
                configurationAction(configuration);
                return configuration.GetBuilder(_modelManager);
            };
            return this;
        }

        /// <summary>
        /// Applies the running configuration to an HttpConfiguration instance
        /// </summary>
        /// <param name="httpConfig">The HttpConfiguration to apply this JsonApiConfiguration to</param>
        public void Apply(HttpConfiguration httpConfig)
        {
            var formatter = new JsonApiFormatter(_modelManager);

            httpConfig.Formatters.Clear();
            httpConfig.Formatters.Add(formatter);

            var queryablePayloadBuilder = _payloadBuilderFactory();
            httpConfig.Filters.Add(new JsonApiQueryableAttribute(queryablePayloadBuilder));

            httpConfig.Services.Replace(typeof (IHttpControllerSelector),
                new PascalizedControllerSelector(httpConfig));
        }
    }
}
