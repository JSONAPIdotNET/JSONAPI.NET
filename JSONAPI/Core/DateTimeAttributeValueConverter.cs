using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// use converting between DateTime CLR properties and ISO8601 string values.
    /// </summary>
    public class DateTimeAttributeValueConverter : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;
        private readonly bool _isNullable;

        /// <summary>
        /// Creates a new DateTimeAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        /// <param name="isNullable"></param>
        public DateTimeAttributeValueConverter(PropertyInfo property, bool isNullable)
        {
            _property = property;
            _isNullable = isNullable;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value != null) return ((DateTime) value).ToString("s");
            if (_isNullable) return null;
            return "0001-01-01";
        }

        public void SetValue(object resource, JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                _property.SetValue(resource, _isNullable ? (DateTime?)null : new DateTime());
            }
            else
            {
                var dateTimeValue = value.Value<DateTime>();
                _property.SetValue(resource, dateTimeValue);
            }
        }
    }
}