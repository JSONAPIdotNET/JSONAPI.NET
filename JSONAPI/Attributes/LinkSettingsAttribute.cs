using System;

namespace JSONAPI.Attributes
{
    /// <summary>
    /// This attribute should be added to a property to override DefaultLinkConventions's default
    /// behavior for serializing links.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LinkSettingsAttribute : Attribute
    {
        internal bool SerializeRelationshipLink;

        internal bool SerializeRelatedResourceLink;

        /// <summary>
        /// Creates a new LinkSettingsAttribute.
        /// </summary>
        public LinkSettingsAttribute(bool serializeRelationshipLink = true, bool serializeRelatedResourceLink = true)
        {
            SerializeRelationshipLink = serializeRelationshipLink;
            SerializeRelatedResourceLink = serializeRelatedResourceLink;
        }
    }
}
