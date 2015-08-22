using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// primitive types.
    /// </summary>
    public class PrimitiveTypeAttributeValueConverter<T> : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;

        /// <summary>
        /// Creates a new PrimitiveTypeAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        public PrimitiveTypeAttributeValueConverter(PropertyInfo property)
        {
            _property = property;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value == null) return null;
            return JToken.FromObject(value);
        }

        public void SetValue(object resource, JToken value)
        {
            if (value == null)
            {
                _property.SetValue(resource, null);
            }
            else
            {
                var unpackedValue = value.Value<T>();
                _property.SetValue(resource, unpackedValue);
            }
        }
    }
}