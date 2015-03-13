using JSONAPI.Core;
using JSONAPI.EntityFramework.ActionFilters;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// Extension Methods for JSONAPI.JsonApiConfiguration
    /// </summary>
    public static class JsonApiConfigurationExtensions
    {
        /// <summary>
        /// Add Entity Framework specific handling to the configuration
        /// </summary>
        /// <param name="jsonApiConfig">The configuration object to modify</param>
        /// <returns>The same configuration object that was passed in</returns>
        public static JsonApiConfiguration UseEntityFramework(this JsonApiConfiguration jsonApiConfig)
        {
            jsonApiConfig.EnumerateQueriesWith(new AsynchronousEnumerationTransformer());

            return jsonApiConfig;
        }
    }
}
