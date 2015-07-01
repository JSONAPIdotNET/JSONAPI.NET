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
        private readonly IMetadataFormatter _metadataFormatter;
        private const string HrefKeyName = "href";
        private const string MetaKeyName = "meta";

        /// <summary>
        /// Constructs a LinkFormatter
        /// </summary>
        /// <param name="metadataFormatter"></param>
        public LinkFormatter(IMetadataFormatter metadataFormatter)
        {
            _metadataFormatter = metadataFormatter;
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
                _metadataFormatter.Serialize(link.Metadata, writer);
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