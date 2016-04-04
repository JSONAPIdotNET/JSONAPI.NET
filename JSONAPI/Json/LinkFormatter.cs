using System;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of ILinkFormatter
    /// </summary>
    public class LinkFormatter : ILinkFormatter
    {
        private IMetadataFormatter _metadataFormatter;
        private const string HrefKeyName = "href";
        private const string MetaKeyName = "meta";

        /// <summary>
        /// Constructs a LinkFormatter
        /// </summary>
        public LinkFormatter()
        {
            
        }

        /// <summary>
        /// Constructs a LinkFormatter
        /// </summary>
        /// <param name="metadataFormatter"></param>
        public LinkFormatter(IMetadataFormatter metadataFormatter)
        {
            _metadataFormatter = metadataFormatter;
        }

        /// <summary>
        /// The formatter to use for metadata on the link object
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IMetadataFormatter MetadataFormatter
        {
            get
            {
                return _metadataFormatter ?? (_metadataFormatter = new MetadataFormatter());
            }
            set
            {
                if (_metadataFormatter != null) throw new InvalidOperationException("This property can only be set once.");
                _metadataFormatter = value;
            }
        }

        public Task Serialize(ILink link, JsonWriter writer)
        {
            if (link.Metadata == null)
            {
                writer.WriteValue(link.Href);
            }
            else
            {
                writer.WriteStartObject();
                writer.WritePropertyName(HrefKeyName);
                writer.WriteValue(link.Href);
                writer.WritePropertyName(MetaKeyName);
                MetadataFormatter.Serialize(link.Metadata, writer);
                writer.WriteEndObject();
            }
            return Task.FromResult(0);
        }

        public Task<ILink> Deserialize(JsonReader reader, string currentPath)
        {
            // The client should never be sending us links.
            throw new NotSupportedException();
        }
    }
}