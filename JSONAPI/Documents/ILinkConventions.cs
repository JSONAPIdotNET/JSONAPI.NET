using JSONAPI.Core;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Service to provide formatting of links
    /// </summary>
    public interface ILinkConventions
    {
        /// <summary>
        /// Builds a relationship link for the given model property
        /// </summary>
        /// <param name="relationshipOwner">The resource that this relationship belongs to.</param>
        /// <param name="resourceTypeRegistry">The model manage to use to lookup information about the relationship owner's type</param>
        /// <param name="property">The property to get the link for</param>
        /// <param name="baseUrl">The portion of the URL that the link will be appended to</param>
        /// <returns></returns>
        ILink GetRelationshipLink<TResource>(TResource relationshipOwner, IResourceTypeRegistry resourceTypeRegistry, ResourceTypeRelationship property, string baseUrl);

        /// <summary>
        /// Builds a related resource link for the given model property
        /// </summary>
        /// <param name="relationshipOwner">The resource that this relationship belongs to.</param>
        /// <param name="resourceTypeRegistry">The model manage to use to lookup information about the relationship owner's type</param>
        /// <param name="property">The property to get the link for</param>
        /// <param name="baseUrl">The portion of the URL that the link will be appended to</param>
        /// <returns></returns>
        ILink GetRelatedResourceLink<TResource>(TResource relationshipOwner, IResourceTypeRegistry resourceTypeRegistry, ResourceTypeRelationship property, string baseUrl);
    }
}
