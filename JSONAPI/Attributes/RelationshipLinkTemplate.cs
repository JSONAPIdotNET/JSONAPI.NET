using System;

namespace JSONAPI.Attributes
{
    /// <summary>
    /// This attribute should be added to a property to override DefaultLinkConventions's default
    /// behavior for generating a related resource link template. The string "{1}" will be replaced
    /// by the ID of the resource that owns the relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RelationshipLinkTemplate : Attribute
    {
        internal string TemplateString;

        /// <summary>
        /// Creates a new RelationshipLinkTemplate.
        /// </summary>
        /// <param name="linkTemplate">The template</param>
        public RelationshipLinkTemplate(string linkTemplate)
        {
            TemplateString = linkTemplate;
        }
    }
}