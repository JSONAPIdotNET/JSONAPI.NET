using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// use converting between Guid CLR properties and string attributes.
    /// </summary>
    public class GuidAttributeValueConverter : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;
        private readonly bool _isNullable;

        /// <summary>
        /// Creates a new GuidAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        /// <param name="isNullable"></param>
        public GuidAttributeValueConverter(PropertyInfo property, bool isNullable)
        {
            _property = property;
            _isNullable = isNullable;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value == null)
            {
                if (_isNullable) return null;
                value = new Guid();
            }
            return value.ToString();
        }

        public void SetValue(object resource, JToken value)
        {
            if (value == null)
            {
                _property.SetValue(resource, _isNullable ? (Guid?)null : new Guid());
            }
            else
            {
                var stringTokenValue = value.Value<string>();
                var guidValue = new Guid(stringTokenValue);
                _property.SetValue(resource, guidValue);
            }
        }
    }
}