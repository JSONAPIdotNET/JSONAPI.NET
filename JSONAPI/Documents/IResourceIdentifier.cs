namespace JSONAPI.Documents
{
    /// <summary>
    /// Type/ID pair that identifies a particular resource
    /// </summary>
    public interface IResourceIdentifier
    {
        /// <summary>
        /// The type of resource
        /// </summary>
        string Type { get; }

        /// <summary>
        /// The ID of the resource
        /// </summary>
        string Id { get; }
    }
}