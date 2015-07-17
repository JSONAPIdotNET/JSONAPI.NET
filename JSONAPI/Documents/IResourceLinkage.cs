namespace JSONAPI.Documents
{
    /// <summary>
    /// Describes a relationship's linkage
    /// </summary>
    public interface IResourceLinkage
    {
        /// <summary>
        /// Whether the linkage is to-many (true) or to-one (false).
        /// </summary>
        bool IsToMany { get; }

        /// <summary>
        /// The identifiers this linkage refers to. If IsToMany is true, this
        /// property must return an array of length either 0 or 1. If false,
        /// the array may be of any length. This property must not return null.
        /// </summary>
        IResourceIdentifier[] Identifiers { get; }
    }
}