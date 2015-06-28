using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Payload;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IPayloadSerializer
    /// </summary>
    public class SingleResourcePayloadSerializer : ISingleResourcePayloadSerializer
    {
        private readonly IResourceObjectSerializer _resourceObjectSerializer;
        private readonly IMetadataSerializer _metadataSerializer;
        private const string PrimaryDataKeyName = "data";
        private const string RelatedDataKeyName = "included";
        private const string MetaKeyName = "meta";

        /// <summary>
        /// Creates a SingleResourcePayloadSerializer
        /// </summary>
        /// <param name="resourceObjectSerializer"></param>
        /// <param name="metadataSerializer"></param>
        public SingleResourcePayloadSerializer(IResourceObjectSerializer resourceObjectSerializer, IMetadataSerializer metadataSerializer)
        {
            _resourceObjectSerializer = resourceObjectSerializer;
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(ISingleResourcePayload payload, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(PrimaryDataKeyName);

            _resourceObjectSerializer.Serialize(payload.PrimaryData, writer);

            if (payload.RelatedData != null && payload.RelatedData.Any())
            {
                writer.WritePropertyName(RelatedDataKeyName);
                writer.WriteStartArray();
                foreach (var resourceObject in payload.RelatedData)
                {
                    _resourceObjectSerializer.Serialize(resourceObject, writer);
                }
                writer.WriteEndArray();
            }

            if (payload.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                _metadataSerializer.Serialize(payload.Metadata, writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        public async Task<ISingleResourcePayload> Deserialize(JsonReader reader, string currentPath)
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
                        metadata = await _metadataSerializer.Deserialize(reader, currentPath + "/" + MetaKeyName);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new SingleResourcePayload(primaryData, new IResourceObject[] { }, metadata);
        }

        private async Task<IResourceObject> DeserializePrimaryData(JsonReader reader, string currentPath)
        {
            if (reader.TokenType == JsonToken.Null) return null;

            var primaryData = await _resourceObjectSerializer.Deserialize(reader, currentPath);
            return primaryData;
        }
    }
}