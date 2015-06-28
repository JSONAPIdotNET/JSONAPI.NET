using System;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Core
{
    /// <summary>
    /// Stores a resource type's field and its usage.
    /// </summary>
    public abstract class ResourceTypeField
    {
        internal ResourceTypeField(PropertyInfo property, string jsonKey)
        {
            JsonKey = jsonKey;
            Property = property;
        }

        /// <summary>
        /// The PropertyInfo backing this ModelProperty
        /// </summary>
        public PropertyInfo Property { get; private set; }

        /// <summary>
        /// The key that will be used to represent this property in JSON API documents
        /// </summary>
        public string JsonKey { get; private set; }
    }

    /// <summary>
    /// A ResourceTypeField representing an attribute on a resource object
    /// </summary>
    public class ResourceTypeAttribute : ResourceTypeField
    {
        private readonly IAttributeValueConverter _attributeValueConverter;

        internal ResourceTypeAttribute(IAttributeValueConverter attributeValueConverter, PropertyInfo property, string jsonKey)
            : base(property, jsonKey)
        {
            _attributeValueConverter = attributeValueConverter;
        }

        /// <summary>
        /// Gets the json-formatted value of this attribute for the given resource
        /// </summary>
        /// <param name="resource"></param>
        /// <returns></returns>
        public JToken GetValue(object resource)
        {
            return _attributeValueConverter.GetValue(resource);
        }

        /// <summary>
        /// Sets the value of this attribute for the resource
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="value"></param>
        public void SetValue(object resource, JToken value)
        {
            _attributeValueConverter.SetValue(resource, value);
        }
    }

    /// <summary>
    /// A ResourceTypeField representing a relationship to another resource type
    /// </summary>
    public abstract class ResourceTypeRelationship : ResourceTypeField
    {
        internal ResourceTypeRelationship(PropertyInfo property, string jsonKey, Type relatedType,
            string selfLinkTemplate, string relatedResourceLinkTemplate, bool isToMany)
            : base(property, jsonKey)
        {
            RelatedType = relatedType;
            SelfLinkTemplate = selfLinkTemplate;
            RelatedResourceLinkTemplate = relatedResourceLinkTemplate;
            IsToMany = isToMany;
        }

        /// <summary>
        /// Whether this relationship represents a link to a collection of resources or a single one.
        /// </summary>
        public bool IsToMany { get; private set; }

        /// <summary>
        /// The type of resource found on the other side of this relationship
        /// </summary>
        public Type RelatedType { get; private set; }

        /// <summary>
        /// The template for building URLs to access the relationship itself.
        /// If the string {1} appears in the template, it will be replaced by the ID of resource this
        /// relationship belongs to.
        /// </summary>
        public string SelfLinkTemplate { get; private set; }

        /// <summary>
        /// The template for building URLs to access the data making up the other side of this relationship.
        /// If the string {1} appears in the template, it will be replaced by the ID of resource this
        /// relationship belongs to.
        /// </summary>
        public string RelatedResourceLinkTemplate { get; private set; }
    }

    /// <summary>
    /// A ModelProperty representing a relationship to a collection of resources
    /// </summary>
    public sealed class ToManyResourceTypeRelationship : ResourceTypeRelationship
    {
        internal ToManyResourceTypeRelationship(PropertyInfo property, string jsonKey, Type relatedType,
            string selfLinkTemplate, string relatedResourceLinkTemplate)
            : base(property, jsonKey, relatedType, selfLinkTemplate, relatedResourceLinkTemplate, true)
        {
        }
    }

    /// <summary>
    /// A ModelProperty representing a relationship to a single resource
    /// </summary>
    public sealed class ToOneResourceTypeRelationship : ResourceTypeRelationship
    {
        internal ToOneResourceTypeRelationship(PropertyInfo property, string jsonKey, Type relatedType,
            string selfLinkTemplate, string relatedResourceLinkTemplate)
            : base(property, jsonKey, relatedType, selfLinkTemplate, relatedResourceLinkTemplate, false)
        {
        }
    }
}
