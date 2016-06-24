using System;
using System.Reflection;

namespace JSONAPI.Core
{
    /// <summary>
    /// A ModelProperty representing a relationship to a single resource
    /// </summary>
    public sealed class ToOneResourceTypeRelationship : ResourceTypeRelationship
    {
        internal ToOneResourceTypeRelationship(PropertyInfo property, string jsonKey, Type relatedType,
            string selfLinkTemplate, string relatedResourceLinkTemplate, bool serializeRelationshipLink = true, bool serializeRelatedResourceLink = true)
            : base(property, jsonKey, relatedType, selfLinkTemplate, relatedResourceLinkTemplate, false, serializeRelationshipLink, serializeRelatedResourceLink)
        {
        }
    }
}