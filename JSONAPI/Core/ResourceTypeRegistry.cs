using System.Collections.ObjectModel;
using JSONAPI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JSONAPI.Extensions;
using Newtonsoft.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Allows configuring how to calculate JSON API keys based on CLR types and properties
    /// </summary>
    public interface INamingConventions
    {
        /// <summary>
        /// Calculates the field name for a given property
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        string GetFieldNameForProperty(PropertyInfo property);

        /// <summary>
        /// Calculates the resource type name for a CLR type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        string GetResourceTypeNameForType(Type type);
    }

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

    /// <summary>
    /// Default implementation of IModelRegistry
    /// </summary>
    public class ResourceTypeRegistry : IResourceTypeRegistry
    {
        private readonly INamingConventions _namingConventions;

        /// <summary>
        /// Creates a new ResourceTypeRegistry
        /// </summary>
        /// <param name="namingConventions"></param>
        public ResourceTypeRegistry(INamingConventions namingConventions)
        {
            _namingConventions = namingConventions;
            RegistrationsByName = new Dictionary<string, ResourceTypeRegistration>();
            RegistrationsByType = new Dictionary<Type, ResourceTypeRegistration>();
        }

        /// <summary>
        /// Represents a type's registration with a model manager
        /// </summary>
        protected sealed class ResourceTypeRegistration : IResourceTypeRegistration
        {
            private readonly IReadOnlyDictionary<string, ResourceTypeField> _fields;
            private readonly Func<ParameterExpression, string, BinaryExpression> _filterByIdExpressionFactory;
            private readonly Func<ParameterExpression, Expression> _sortByIdExpressionFactory;

            internal ResourceTypeRegistration(Type type, PropertyInfo idProperty, string resourceTypeName,
                IDictionary<string, ResourceTypeField> fields,
                Func<ParameterExpression, string, BinaryExpression> filterByIdExpressionFactory,
                Func<ParameterExpression, Expression> sortByIdExpressionFactory)
            {
                IdProperty = idProperty;
                Type = type;
                ResourceTypeName = resourceTypeName;
                _filterByIdExpressionFactory = filterByIdExpressionFactory;
                _sortByIdExpressionFactory = sortByIdExpressionFactory;
                Attributes = fields.Values.OfType<ResourceTypeAttribute>().ToArray();
                Relationships = fields.Values.OfType<ResourceTypeRelationship>().ToArray();
                _fields = new ReadOnlyDictionary<string, ResourceTypeField>(fields);
            }

            public Type Type { get; private set; }

            public PropertyInfo IdProperty { get; private set; }

            public string ResourceTypeName { get; private set; }

            public ResourceTypeAttribute[] Attributes { get; private set; }

            public ResourceTypeRelationship[] Relationships { get; private set; }

            public string GetIdForResource(object resource)
            {
                return IdProperty.GetValue(resource).ToString();
            }

            public void SetIdForResource(object resource, string id)
            {
                IdProperty.SetValue(resource, id); // TODO: handle classes with non-string ID types
            }

            public BinaryExpression GetFilterByIdExpression(ParameterExpression parameter, string id)
            {
                return _filterByIdExpressionFactory(parameter, id);
            }

            public Expression GetSortByIdExpression(ParameterExpression parameter)
            {
                return _sortByIdExpressionFactory(parameter);
            }

            public ResourceTypeField GetFieldByName(string name)
            {
                return _fields.ContainsKey(name) ? _fields[name] : null;
            }
        }

        protected readonly IDictionary<string, ResourceTypeRegistration> RegistrationsByName;
        protected readonly IDictionary<Type, ResourceTypeRegistration> RegistrationsByType; 

        public bool TypeIsRegistered(Type type)
        {
            var registration = FindRegistrationForType(type);
            return registration != null;
        }

        public IResourceTypeRegistration GetRegistrationForType(Type type)
        {
            var reg = FindRegistrationForType(type);
            if (reg == null)
                throw new TypeRegistrationNotFoundException(type);

            return reg;
        }

        public IResourceTypeRegistration GetRegistrationForResourceTypeName(string resourceTypeName)
        {
            lock (RegistrationsByName)
            {
                ResourceTypeRegistration registration;
                if (!RegistrationsByName.TryGetValue(resourceTypeName, out registration))
                    throw new TypeRegistrationNotFoundException(resourceTypeName);

                return registration;
            }
        }

        private ResourceTypeRegistration FindRegistrationForType(Type type)
        {
            lock (RegistrationsByType)
            {
                var currentType = type;
                while (currentType != null && currentType != typeof(Object))
                {
                    ResourceTypeRegistration registration;
                    if (RegistrationsByType.TryGetValue(currentType, out registration))
                        return registration;

                    // This particular type wasn't registered, but maybe the base type was.
                    currentType = currentType.BaseType;
                }
            }

            return null;
        }

        /// <summary>
        /// Registeres a type with this ModelManager, using a default resource type name.
        /// </summary>
        /// <param name="type">The type to register.</param>
        /// <param name="resourceTypeName">The resource type name to use</param>
        /// <param name="filterByIdFactory">The factory to use to build an expression that </param>
        /// <param name="sortByIdFactory"></param>
        public ResourceTypeRegistry RegisterResourceType(Type type, string resourceTypeName = null,
            Func<ParameterExpression, string, BinaryExpression> filterByIdFactory = null, Func<ParameterExpression, Expression> sortByIdFactory = null)
        {
            lock (RegistrationsByType)
            {
                lock (RegistrationsByName)
                {
                    if (resourceTypeName == null)
                        resourceTypeName = _namingConventions.GetResourceTypeNameForType(type);

                    if (RegistrationsByType.ContainsKey(type))
                        throw new InvalidOperationException(String.Format("The type `{0}` has already been registered.",
                            type.FullName));

                    if (RegistrationsByName.ContainsKey(resourceTypeName))
                        throw new InvalidOperationException(
                            String.Format("The resource type name `{0}` has already been registered.", resourceTypeName));

                    var fieldMap = new Dictionary<string, ResourceTypeField>();

                    var idProperty = CalculateIdProperty(type);
                    if (idProperty == null)
                        throw new InvalidOperationException(String.Format(
                            "Unable to determine Id property for type `{0}`.", type.Name));

                    var props = type.GetProperties().OrderBy(p => p.Name);
                    foreach (var prop in props)
                    {
                        if (prop == idProperty) continue;

                        var ignore = prop.CustomAttributes.Any(c => c.AttributeType == typeof(JsonIgnoreAttribute));
                        if (ignore) continue;

                        var property = CreateResourceTypeField(prop);
                        var jsonKey = property.JsonKey;

                        if (jsonKey == "id")
                            throw new InvalidOperationException(
                                String.Format("Failed to register type `{0}` because it contains a non-id property that would serialize as \"id\".", type.Name));

                        if (jsonKey == "type")
                            throw new InvalidOperationException(
                                String.Format("Failed to register type `{0}` because it contains a property that would serialize as \"type\".", type.Name));

                        if (fieldMap.ContainsKey(jsonKey))
                            throw new InvalidOperationException(
                                String.Format("Failed to register type `{0}` because contains multiple properties that would serialize as `{1}`.",
                                    type.Name, jsonKey));

                        fieldMap[jsonKey] = property;
                    }

                    if (filterByIdFactory == null)
                    {
                        filterByIdFactory = (param, id) =>
                        {
                            var propertyExpr = Expression.Property(param, idProperty);
                            var idExpr = Expression.Constant(id);
                            return Expression.Equal(propertyExpr, idExpr);
                        };
                    }

                    if (sortByIdFactory == null)
                    {
                        sortByIdFactory = param => Expression.Property(param, idProperty);
                    }

                    var registration = new ResourceTypeRegistration(type, idProperty, resourceTypeName, fieldMap, filterByIdFactory, sortByIdFactory);

                    RegistrationsByType.Add(type, registration);
                    RegistrationsByName.Add(resourceTypeName, registration);
                }
            }

            return this;
        }

        /// <summary>
        /// Gets a value converter for the given property
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        protected virtual IAttributeValueConverter GetValueConverterForProperty(PropertyInfo prop)
        {
            var serializeAsComplexAttribute = prop.GetCustomAttribute<SerializeAsComplexAttribute>();
            if (serializeAsComplexAttribute != null)
                return new ComplexAttributeValueConverter(prop);

            if (prop.PropertyType == typeof(DateTime))
                return new DateTimeAttributeValueConverter(prop, false);

            if (prop.PropertyType == typeof(DateTime?))
                return new DateTimeAttributeValueConverter(prop, true);

            if (prop.PropertyType == typeof(DateTimeOffset))
                return new DateTimeOffsetAttributeValueConverter(prop, false);

            if (prop.PropertyType == typeof(DateTimeOffset?))
                return new DateTimeOffsetAttributeValueConverter(prop, true);

            if (prop.PropertyType == typeof (Decimal) || prop.PropertyType == typeof (Decimal?))
                return new DecimalAttributeValueConverter(prop);

            if (prop.PropertyType == typeof (Guid))
                return new GuidAttributeValueConverter(prop, false);

            if (prop.PropertyType == typeof(Guid?))
                return new GuidAttributeValueConverter(prop, true);

            if (prop.PropertyType.IsEnum)
                return new EnumAttributeValueConverter(prop, prop.PropertyType, true);

            Type enumType;
            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof (Nullable<>) &&
                (enumType = prop.PropertyType.GetGenericArguments()[0]).IsEnum)
            {
                return new EnumAttributeValueConverter(prop, enumType, true);
            }

            var closedType = typeof(PrimitiveTypeAttributeValueConverter<>).MakeGenericType(prop.PropertyType);
            return (IAttributeValueConverter)Activator.CreateInstance(closedType, prop);
        }

        /// <summary>
        /// Creates a cacheable model field representation from a PropertyInfo
        /// </summary>
        /// <param name="prop">The property</param>
        /// <returns>A model field represenation</returns>
        protected virtual ResourceTypeField CreateResourceTypeField(PropertyInfo prop)
        {
            var jsonKey = _namingConventions.GetFieldNameForProperty(prop);

            var type = prop.PropertyType;

            if (prop.PropertyType.CanWriteAsJsonApiAttribute())
            {
                var converter = GetValueConverterForProperty(prop);
                return new ResourceTypeAttribute(converter, prop, jsonKey);
            }

            var selfLinkTemplateAttribute = prop.GetCustomAttributes().OfType<RelationshipLinkTemplate>().FirstOrDefault();
            var selfLinkTemplate = selfLinkTemplateAttribute == null ? null : selfLinkTemplateAttribute.TemplateString;
            var relatedResourceLinkTemplateAttribute = prop.GetCustomAttributes().OfType<RelatedResourceLinkTemplate>().FirstOrDefault();
            var relatedResourceLinkTemplate = relatedResourceLinkTemplateAttribute == null ? null : relatedResourceLinkTemplateAttribute.TemplateString;

            var isToMany =
                type.IsArray ||
                (type.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)) && type.IsGenericType);

            if (!isToMany) return new ToOneResourceTypeRelationship(prop, jsonKey, type, selfLinkTemplate, relatedResourceLinkTemplate);
            var relatedType = type.IsGenericType ? type.GetGenericArguments()[0] : type.GetElementType();
            return new ToManyResourceTypeRelationship(prop, jsonKey, relatedType, selfLinkTemplate, relatedResourceLinkTemplate);
        }

        /// <summary>
        /// Calculates the ID property for a given resource type.
        /// </summary>
        /// <param name="type">The type to use to calculate the ID for</param>
        /// <returns>The ID property to use for this type</returns>
        protected virtual PropertyInfo CalculateIdProperty(Type type)
        {
            return
                type
                    .GetProperties()
                    .FirstOrDefault(p => p.CustomAttributes.Any(attr => attr.AttributeType == typeof(UseAsIdAttribute)))
                ?? type.GetProperty("Id");
        }
    }
}
