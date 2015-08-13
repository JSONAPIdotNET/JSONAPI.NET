using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// use converting between enum CLR properties and integer attributes.
    /// </summary>
    public class EnumAttributeValueConverter : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;
        private readonly Type _enumType;
        private readonly bool _isNullable;

        /// <summary>
        /// Creates a new EnumAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        /// <param name="enumType"></param>
        /// <param name="isNullable"></param>
        public EnumAttributeValueConverter(PropertyInfo property, Type enumType, bool isNullable)
        {
            _property = property;
            _enumType = enumType;
            _isNullable = isNullable;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value != null) return (int) value;
            if (_isNullable) return null;
            return 0;
        }

        public void SetValue(object resource, JToken value)
        {
            if (value == null || value.Type == JTokenType.Null)
            {
                if (_isNullable)
                    _property.SetValue(resource, null);
                else
                {
                    var enumValue = Enum.Parse(_enumType, "0");
                    _property.SetValue(resource, enumValue);
                }
            }
            else
            {
                var enumValue = Enum.Parse(_enumType, value.ToString());
                _property.SetValue(resource, enumValue);
            }
        }
    }
}