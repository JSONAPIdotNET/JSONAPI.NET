namespace JSONAPI.Documents
{
    /// <summary>
    /// A link that may be found in a "links object"
    /// </summary>
    /// <see cref="http://jsonapi.org/format/#document-links"/>
    public interface ILink
    {
        /// <summary>
        /// a string containing the link's URL.
        /// </summary>
        string Href { get; }

        /// <summary>
        /// a meta object containing non-standard meta-information about the link.
        /// </summary>
        IMetadata Metadata { get; }
    }

    /// <summary>
    /// Default implementation of ILink
    /// </summary>
    public class Link : ILink
    {
        public string Href { get; private set; }
        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Constructs a link
        /// </summary>
        /// <param name="href">The URL of the link</param>
        /// <param name="metadata">metadata about the link</param>
        public Link(string href, IMetadata metadata)
        {
            Href = href;
            Metadata = metadata;
        }
    }
}
