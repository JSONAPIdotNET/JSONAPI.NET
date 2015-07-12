using System;
using System.Reflection;

namespace JSONAPI.Core
{
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
}