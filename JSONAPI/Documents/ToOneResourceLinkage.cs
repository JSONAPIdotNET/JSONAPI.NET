namespace JSONAPI.Documents
{
    /// <summary>
    /// Describes linkage to a single resource
    /// </summary>
    public class ToOneResourceLinkage : IResourceLinkage
    {
        /// <summary>
        /// Creates a to-one resource linkage object
        /// </summary>
        /// <param name="resourceIdentifier"></param>
        public ToOneResourceLinkage(IResourceIdentifier resourceIdentifier)
        {
            Identifiers = resourceIdentifier != null ? new[] {resourceIdentifier} : new IResourceIdentifier[] {};
        }

        public bool IsToMany { get { return false; } }
        public IResourceIdentifier[] Identifiers { get; private set; }
    }
}