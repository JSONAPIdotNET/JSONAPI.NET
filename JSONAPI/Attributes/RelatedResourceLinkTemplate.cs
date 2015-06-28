using System;

namespace JSONAPI.Attributes
{
    /// <summary>
    /// This attribute should be added to a property to override DefaultLinkConventions's default
    /// behavior for generating a relationship link template. The string "{1}" will be replaced
    /// by the ID of the resource that owns the relationship.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RelatedResourceLinkTemplate : Attribute
    {
        internal string TemplateString;

        /// <summary>
        /// Creates a new RelatedResourceLinkTemplate.
        /// </summary>
        /// <param name="linkTemplate">The template</param>
        public RelatedResourceLinkTemplate(string linkTemplate)
        {
            TemplateString = linkTemplate;
        }
    }
}
