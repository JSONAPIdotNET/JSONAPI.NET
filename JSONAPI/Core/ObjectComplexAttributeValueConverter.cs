using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Implementation of <see cref="IAttributeValueConverter" /> suitable for
    /// use with complex attributes that deserialize to custom types.
    /// </summary>
    public class ObjectComplexAttributeValueConverter : IAttributeValueConverter
    {
        private readonly PropertyInfo _property;
        private readonly bool _isToMany;

        /// <summary>
        /// Creates a new ComplexAttributeValueConverter
        /// </summary>
        /// <param name="property"></param>
        /// <param name="isToMany"></param>
        public ObjectComplexAttributeValueConverter(PropertyInfo property, bool isToMany)
        {
            _property = property;
            _isToMany = isToMany;
        }

        public JToken GetValue(object resource)
        {
            var value = _property.GetValue(resource);
            if (value == null) return null;
            return _isToMany ? (JToken)JArray.FromObject(value) : JObject.FromObject(value);
        }

        public void SetValue(object resource, JToken value)
        {
            var deserialized = value?.ToObject(_property.PropertyType);
            _property.SetValue(resource, deserialized);
        }
    }
}