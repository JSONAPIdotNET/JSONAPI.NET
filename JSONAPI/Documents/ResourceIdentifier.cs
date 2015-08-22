namespace JSONAPI.Documents
{
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