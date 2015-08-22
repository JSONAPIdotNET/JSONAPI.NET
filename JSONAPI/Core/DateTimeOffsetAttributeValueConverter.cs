using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// use converting between DateTimeOffset CLR properties and ISO8601 string values.
    /// </summary>
    public class DateTimeOffsetAttributeValueConverter : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;
        private readonly bool _isNullable;

        /// <summary>
        /// Creates a new DateTimeOffsetAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        /// <param name="isNullable"></param>
        public DateTimeOffsetAttributeValueConverter(PropertyInfo property, bool isNullable)
        {
            _property = property;
            _isNullable = isNullable;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value != null) return ((DateTimeOffset)value).ToString("o");
            if (_isNullable) return null;
            return "0001-01-01T00:00:00Z";
        }

        public void SetValue(object resource, JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                _property.SetValue(resource, _isNullable ? (DateTimeOffset?)null : new DateTimeOffset());
            }
            else
            {
                var stringValue = value.Value<string>();
                var dateTimeOffsetValue = DateTimeOffset.Parse(stringValue);
                _property.SetValue(resource, dateTimeOffsetValue);
            }
        }
    }
}