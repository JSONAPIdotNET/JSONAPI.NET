using JSONAPI.Core;
using JSONAPI.EntityFramework.ActionFilters;

namespace JSONAPI.EntityFramework
{
    /// <summary>
    /// Extension Methods for JSONAPI.Core.DefaultQueryablePayloadBuilderConfiguration
    /// </summary>
    public static class DefaultQueryablePayloadBuilderConfigurationExtensions
    {
        /// <summary>
        /// Add Entity Framework specific handling to the configuration
        /// </summary>
        /// <param name="config">The configuration object to modify</param>
        /// <returns>The same configuration object that was passed in</returns>
        public static DefaultQueryablePayloadBuilderConfiguration EnumerateQueriesAsynchronously(this DefaultQueryablePayloadBuilderConfiguration config)
        {
            config.EnumerateQueriesWith(new AsynchronousEnumerationTransformer());

            return config;
        }
    }
}
