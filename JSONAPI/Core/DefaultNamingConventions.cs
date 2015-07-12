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
        public string GetFieldNameForProperty(PropertyInfo property)
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
        public string GetResourceTypeNameForType(Type type)
        {
            var attrs = type.CustomAttributes.Where(x => x.AttributeType == typeof(JsonObjectAttribute)).ToList();

            string title = type.Name;
            if (attrs.Any())
            {
                var titles = attrs.First().NamedArguments.Where(arg => arg.MemberName == "Title")
                    .Select(arg => arg.TypedValue.Value.ToString()).ToList();
                if (titles.Any()) title = titles.First();
            }

            return _pluralizationService.Pluralize(title).Dasherize();
        }
    }
}