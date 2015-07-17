using System;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Describes linkage to a collection of resources
    /// </summary>
    public class ToManyResourceLinkage : IResourceLinkage
    {
        /// <summary>
        /// Creates a To-many resource linkage object
        /// </summary>
        /// <param name="resourceIdentifiers"></param>
        /// <exception cref="NotImplementedException"></exception>
        public ToManyResourceLinkage(IResourceIdentifier[] resourceIdentifiers)
        {
            Identifiers = resourceIdentifiers ?? new IResourceIdentifier[] {};
        }

        public bool IsToMany { get { return true; } }
        public IResourceIdentifier[] Identifiers { get; private set; }
    }
}