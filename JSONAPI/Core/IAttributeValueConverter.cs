using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Provides services for interacting with JSON API attributes
    /// </summary>
    public interface IAttributeValueConverter
    {
        /// <summary>
        /// Gets the json-formatted value of this attribute for the given resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        JToken GetValue(object resource);

        /// <summary>
        /// Sets the value of this attribute for the resource
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="value"></param>
        void SetValue(object resource, JToken value);
    }
}