using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JSONAPI.Attributes;
using JSONAPI.Configuration;
using JSONAPI.Extensions;
using Newtonsoft.Json;

namespace JSONAPI.Core
{
    /// <summary>
    /// Default implementation of <see cref="IResourceTypeRegistrar"/>
    /// </summary>
    public class ResourceTypeRegistrar : IResourceTypeRegistrar
    {
        private readonly INamingConventions _namingConventions;

        /// <summary>
        /// Creates a new <see cref="ResourceTypeRegistrar"/>
        /// </summary>
        /// <param name="namingConventions">Conventions for naming types and fields</param>
        public ResourceTypeRegistrar(INamingConventions namingConventions)
        {
            if (namingConventions == null) throw new ArgumentNullException("namingConventions");
            _namingConventions = namingConventions;
        }

        public IResourceTypeRegistration BuildRegistration(Type type, string resourceTypeName = null,
            Func<ParameterExpression, string, BinaryExpression> filterByIdFactory = null,
            Func<ParameterExpression, Expression> sortByIdFactory = null)
        {
            if (resourceTypeName == null)
                resourceTypeName = _namingConventions.GetResourceTypeNameForType(type);

            var fieldMap = new Dictionary<string, ResourceTypeField>();

            var idProperty = CalculateIdProperty(type);
            if (idProperty == null)
                throw new InvalidOperationException(String.Format(
                    "Unable to determine Id property for type `{0}`.", type.Name));

            var props = type.GetProperties().OrderBy(p => p.Name);
            foreach (var prop in props)
            {
                if (prop == idProperty) continue;

                var ignore = prop.CustomAttributes.Any(c => c.AttributeType == typeof (JsonIgnoreAttribute));
                if (ignore) continue;

                var property = CreateResourceTypeField(prop);
                var jsonKey = property.JsonKey;

                if (jsonKey == "id")
                    throw new InvalidOperationException(
                        String.Format(
                            "Failed to register type `{0}` because it contains a non-id property that would serialize as \"id\".",
                            type.Name));

                if (jsonKey == "type")
                    throw new InvalidOperationException(
                        String.Format(
                            "Failed to register type `{0}` because it contains a property that would serialize as \"type\".",
                            type.Name));

                if (fieldMap.ContainsKey(jsonKey))
                    throw new InvalidOperationException(
                        String.Format(
                            "Failed to register type `{0}` because contains multiple properties that would serialize as `{1}`.",
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

            return new ResourceTypeRegistration(type, idProperty, resourceTypeName, fieldMap, filterByIdFactory,
                sortByIdFactory);
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

            if (prop.PropertyType == typeof(Decimal) || prop.PropertyType == typeof(Decimal?))
                return new DecimalAttributeValueConverter(prop);

            if (prop.PropertyType == typeof(Guid))
                return new GuidAttributeValueConverter(prop, false);

            if (prop.PropertyType == typeof(Guid?))
                return new GuidAttributeValueConverter(prop, true);

            if (prop.PropertyType.IsEnum)
                return new EnumAttributeValueConverter(prop, prop.PropertyType, true);

            Type enumType;
            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) &&
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