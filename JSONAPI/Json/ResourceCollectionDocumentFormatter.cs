using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IResourceCollectionDocumentFormatter
    /// </summary>
    public class ResourceCollectionDocumentFormatter : IResourceCollectionDocumentFormatter
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
        public ResourceCollectionDocumentFormatter(IResourceObjectFormatter resourceObjectFormatter, IMetadataFormatter metadataFormatter)
        {
            _resourceObjectFormatter = resourceObjectFormatter;
            _metadataFormatter = metadataFormatter;
        }

        public Task Serialize(IResourceCollectionDocument document, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(PrimaryDataKeyName);

            writer.WriteStartArray();
            foreach (var resourceObject in document.PrimaryData)
            {
                _resourceObjectFormatter.Serialize(resourceObject, writer);
            }
            writer.WriteEndArray();

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

            writer.Flush();

            return Task.FromResult(0);
        }

        public async Task<IResourceCollectionDocument> Deserialize(JsonReader reader, string currentPath)
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
                        metadata = await _metadataFormatter.Deserialize(reader, currentPath + "/" + MetaKeyName);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            return new ResourceCollectionDocument(primaryData ?? new IResourceObject[] { }, new IResourceObject[] { }, metadata);
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

                var resourceObject = await _resourceObjectFormatter.Deserialize(reader, currentPath + "/" + index);
                primaryData.Add(resourceObject);

                index++;
            }

            return primaryData.ToArray();
        }
    }
}