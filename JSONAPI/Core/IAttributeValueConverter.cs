using System;
using System.Reflection;
using Newtonsoft.Json;
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
            if (value == null)
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
            if (value == null)
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
            if (value == null)
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