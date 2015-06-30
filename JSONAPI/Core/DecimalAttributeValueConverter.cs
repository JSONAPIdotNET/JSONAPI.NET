using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// use converting between decimal CLR properties and string attributes.
    /// </summary>
    public class DecimalAttributeValueConverter : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;

        /// <summary>
        /// Creates a new DecimalAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        public DecimalAttributeValueConverter(PropertyInfo property)
        {
            _property = property;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value == null) return null;
            return value.ToString();
        }

        public void SetValue(object resource, JToken value)
        {
            if (value == null)
                _property.SetValue(resource, null);
            else
            {
                var stringTokenValue = value.Value<string>();
                Decimal d;
                if (!Decimal.TryParse(stringTokenValue, out d))
                    throw new JsonSerializationException("Could not parse decimal value.");
                _property.SetValue(resource, d);
            }
        }
    }
}