using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// A ResourceTypeField representing an attribute on a resource object
    /// </summary>
    public class ResourceTypeAttribute : ResourceTypeField
    {
        private readonly IAttributeValueConverter _attributeValueConverter;

        internal ResourceTypeAttribute(IAttributeValueConverter attributeValueConverter, PropertyInfo property, string jsonKey)
            : base(property, jsonKey)
        {
            _attributeValueConverter = attributeValueConverter;
        }

        /// <summary>
        /// Gets the json-formatted value of this attribute for the given resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public JToken GetValue(object resource)
        {
            return _attributeValueConverter.GetValue(resource);
        }

        /// <summary>
        /// Sets the value of this attribute for the resource
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="value"></param>
        public void SetValue(object resource, JToken value)
        {
            _attributeValueConverter.SetValue(resource, value);
        }
    }
}