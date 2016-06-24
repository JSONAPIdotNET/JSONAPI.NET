using System;
using System.Reflection;

namespace JSONAPI.Core
{
    /// <summary>
    /// A ResourceTypeField representing a relationship to another resource type
    /// </summary>
    public abstract class ResourceTypeRelationship : ResourceTypeField
    {
        internal ResourceTypeRelationship(PropertyInfo property, string jsonKey, Type relatedType,
            string selfLinkTemplate, string relatedResourceLinkTemplate, bool isToMany,
            bool serializeRelationshipLink = true, bool serializeRelatedResourceLink = true)
            : base(property, jsonKey)
        {
            RelatedType = relatedType;
            SelfLinkTemplate = selfLinkTemplate;
            RelatedResourceLinkTemplate = relatedResourceLinkTemplate;
            IsToMany = isToMany;
            SerializeRelationshipLink = serializeRelationshipLink;
            SerializeRelatedResourceLink = serializeRelatedResourceLink;
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

        /// <summary>
        /// Whether to include a link to the relationship URL when serializing relationship objects for
        /// this relationship
        /// </summary>
        public bool SerializeRelationshipLink { get; private set; }

        /// <summary>
        /// Whether to include a link to the related resource URL when serializing relationship objects for
        /// this relationship
        /// </summary>
        public bool SerializeRelatedResourceLink { get; private set; }
    }
}