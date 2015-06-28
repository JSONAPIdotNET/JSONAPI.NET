using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Payload;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IResourceCollectionPayloadSerializer
    /// </summary>
    public class ResourceCollectionPayloadSerializer : IResourceCollectionPayloadSerializer
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
        public ResourceCollectionPayloadSerializer(IResourceObjectSerializer resourceObjectSerializer, IMetadataSerializer metadataSerializer)
        {
            _resourceObjectSerializer = resourceObjectSerializer;
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(IResourceCollectionPayload payload, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(PrimaryDataKeyName);

            writer.WriteStartArray();
            foreach (var resourceObject in payload.PrimaryData)
            {
                _resourceObjectSerializer.Serialize(resourceObject, writer);
            }
            writer.WriteEndArray();

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

            writer.Flush();

            return Task.FromResult(0);
        }

        public async Task<IResourceCollectionPayload> Deserialize(JsonReader reader, string currentPath)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new JsonSerializationException("Document root is not an object!");

            IResourceObject[] primaryData = null;
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

            return new ResourceCollectionPayload(primaryData ?? new IResourceObject[] { }, new IResourceObject[] { }, metadata);
        }

        private async Task<IResourceObject[]> DeserializePrimaryData(JsonReader reader, string currentPath)
        {
            if (reader.TokenType != JsonToken.StartArray)
                throw new JsonSerializationException("Expected an array, but encountered " + reader.TokenType);

            var primaryData = new List<IResourceObject>();

            var index = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndArray)
                    break;

                var resourceObject = await _resourceObjectSerializer.Deserialize(reader, currentPath + "/" + index);
                primaryData.Add(resourceObject);

                index++;
            }

            return primaryData.ToArray();
        }
    }
}