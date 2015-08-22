using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// use with complex attributes.
    /// </summary>
    public class ComplexAttributeValueConverter : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;

        /// <summary>
        /// Creates a new ComplexAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        public ComplexAttributeValueConverter(PropertyInfo property)
        {
            _property = property;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value == null) return null;
            return JToken.Parse(value.ToString());
        }

        public void SetValue(object resource, JToken value)
        {
            var serializedValue = value.ToString(); // TODO: this won't work if this converter is used for non-string properties
            _property.SetValue(resource, serializedValue);
        }
    }
}