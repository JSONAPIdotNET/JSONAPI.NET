namespace JSONAPI.Core
{
    /// <summary>
    /// Service for creating an instance of a registered resource type
    /// </summary>
    public interface IEphemeralRelatedResourceCreator
    {
        /// <summary>
        /// Creates an instance of the specified resource type, with the given ID
        /// </summary>
        /// <param name="resourceTypeRegistration">The type to create the instance for</param>
        /// <param name="id">The ID for the resource</param>
        /// <returns>A new instance of the specified resource type</returns>
        object CreateEphemeralResource(IResourceTypeRegistration resourceTypeRegistration, string id);
    }
}