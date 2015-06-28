namespace JSONAPI.Payload
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

    /// <summary>
    /// Default implementation of IResourceIdentifier
    /// </summary>
    public class ResourceIdentifier : IResourceIdentifier
    {
        public string Type { get; private set; }
        public string Id { get; private set; }

        /// <summary>
        /// Creates a new ResourceIdentifier
        /// </summary>
        /// <param name="type"></param>
        /// <param name="id"></param>
        public ResourceIdentifier(string type, string id)
        {
            Type = type;
            Id = id;
        }
    }
}