using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of ISingleResourceDocumentFormatter
    /// </summary>
    public class SingleResourceDocumentFormatter : ISingleResourceDocumentFormatter
    {
        private readonly IResourceObjectFormatter _resourceObjectFormatter;
        private readonly IMetadataFormatter _metadataFormatter;
        private const string PrimaryDataKeyName = "data";
        private const string RelatedDataKeyName = "included";
        private const string MetaKeyName = "meta";

        /// <summary>
        /// Creates a SingleResourceDocumentFormatter
        /// </summary>
        /// <param name="resourceObjectFormatter"></param>
        /// <param name="metadataFormatter"></param>
        public SingleResourceDocumentFormatter(IResourceObjectFormatter resourceObjectFormatter, IMetadataFormatter metadataFormatter)
        {
            _resourceObjectFormatter = resourceObjectFormatter;
            _metadataFormatter = metadataFormatter;
        }

        public Task Serialize(ISingleResourceDocument document, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(PrimaryDataKeyName);

            _resourceObjectFormatter.Serialize(document.PrimaryData, writer);

            if (document.RelatedData != null && document.RelatedData.Any())
            {
                writer.WritePropertyName(RelatedDataKeyName);
                writer.WriteStartArray();
                foreach (var resourceObject in document.RelatedData)
                {
                    _resourceObjectFormatter.Serialize(resourceObject, writer);
                }
                writer.WriteEndArray();
            }

            if (document.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                _metadataFormatter.Serialize(document.Metadata, writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        public async Task<ISingleResourceDocument> Deserialize(JsonReader reader, string currentPath)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new DeserializationException("Invalid document root", "Document root is not an object!", currentPath);

            IResourceObject primaryData = null;
            IMetadata metadata = null;

            while (reader.Read())
            {
                if (reader.TokenType != JsonToken.PropertyName) break;

                // Has to be a property name
                var propertyName = (string)reader.Value;
                reader.Read();

                switch (propertyName)
                {
                    case RelatedDataKeyName:
                        // TODO: If we want to capture related resources, this would be the place to do it
                        reader.Skip();
                        break;
                    case PrimaryDataKeyName:
                        primaryData = await DeserializePrimaryData(reader, currentPath + "/" + PrimaryDataKeyName);
                        break;
                    case MetaKeyName:
                        metadata = await _metadataFormatter.Deserialize(reader, currentPath + "/" + MetaKeyName);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new SingleResourceDocument(primaryData, new IResourceObject[] { }, metadata);
        }

        private async Task<IResourceObject> DeserializePrimaryData(JsonReader reader, string currentPath)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var primaryData = await _resourceObjectFormatter.Deserialize(reader, currentPath);
            return primaryData;
        }
    }
}