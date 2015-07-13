using System;
using System.Linq;
using System.Reflection;
using JSONAPI.Extensions;
using Newtonsoft.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Default implementation of INamingConventions
    /// </summary>
    public class DefaultNamingConventions : INamingConventions
    {
        private readonly IPluralizationService _pluralizationService;

        /// <summary>
        /// Creates a new DefaultNamingConventions
        /// </summary>
        /// <param name="pluralizationService"></param>
        public DefaultNamingConventions(IPluralizationService pluralizationService)
        {
            _pluralizationService = pluralizationService;
        }

        /// <summary>
        /// This method first checks if the property has a [JsonProperty] attribute. If so,
        /// it uses the attribute's PropertyName. Otherwise, it falls back to taking the
        /// property's name, and dasherizing it.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public virtual string GetFieldNameForProperty(PropertyInfo property)
        {
            var jsonPropertyAttribute = (JsonPropertyAttribute)property.GetCustomAttributes(typeof(JsonPropertyAttribute)).FirstOrDefault();
            return jsonPropertyAttribute != null ? jsonPropertyAttribute.PropertyName : property.Name.Dasherize();
        }

        /// <summary>
        /// This method first checks if the type has a [JsonObject] attribute. If so,
        /// it uses the attribute's Title. Otherwise it falls back to pluralizing the
        /// type's name using the given <see cref="IPluralizationService" /> and then
        /// dasherizing that value.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public virtual string GetResourceTypeNameForType(Type type)
        {
            var jsonObjectAttribute = type.GetCustomAttributes().OfType<JsonObjectAttribute>().FirstOrDefault();

            string title = null;
            if (jsonObjectAttribute != null)
            {
                title = jsonObjectAttribute.Title;
            }

            if (string.IsNullOrEmpty(title))
            {
                title = GetNameForType(type);
            }

            return _pluralizationService.Pluralize(title).Dasherize();
        }

        /// <summary>
        /// Gets the name for a CLR type.
        /// </summary>
        protected virtual string GetNameForType(Type type)
        {
            return type.Name;
        }
    }
}