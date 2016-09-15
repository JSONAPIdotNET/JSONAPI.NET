using System;
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
        private IResourceObjectFormatter _resourceObjectFormatter;
        private IMetadataFormatter _metadataFormatter;
        private const string PrimaryDataKeyName = "data";
        private const string RelatedDataKeyName = "included";
        private const string MetaKeyName = "meta";

        /// <summary>
        /// Creates a ResourceCollectionDocumentFormatter
        /// </summary>
        public ResourceCollectionDocumentFormatter()
        {
            
        }

        /// <summary>
        /// Creates a ResourceCollectionDocumentFormatter
        /// </summary>
        /// <param name="resourceObjectFormatter"></param>
        /// <param name="metadataFormatter"></param>
        public ResourceCollectionDocumentFormatter(IResourceObjectFormatter resourceObjectFormatter, IMetadataFormatter metadataFormatter)
        {
            _resourceObjectFormatter = resourceObjectFormatter;
            _metadataFormatter = metadataFormatter;
        }

        /// <summary>
        /// The formatter to use for resource objects found in this document
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IResourceObjectFormatter ResourceObjectFormatter
        {
            get
            {
                return _resourceObjectFormatter ?? (_resourceObjectFormatter = new ResourceObjectFormatter());
            }
            set
            {
                if (_resourceObjectFormatter != null) throw new InvalidOperationException("This property can only be set once.");
                _resourceObjectFormatter = value;
            }
        }

        /// <summary>
        /// The formatter to use for document-level metadata
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

        public Task Serialize(IResourceCollectionDocument document, JsonWriter writer)
        {
            writer.WriteStartObject();

            if (document.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                MetadataFormatter.Serialize(document.Metadata, writer);
            }

            writer.WritePropertyName(PrimaryDataKeyName);

            writer.WriteStartArray();
            foreach (var resourceObject in document.PrimaryData)
            {
                ResourceObjectFormatter.Serialize(resourceObject, writer);
            }
            writer.WriteEndArray();

            if (document.RelatedData != null && document.RelatedData.Any())
            {
                writer.WritePropertyName(RelatedDataKeyName);
                writer.WriteStartArray();
                foreach (var resourceObject in document.RelatedData)
                {
                    ResourceObjectFormatter.Serialize(resourceObject, writer);
                }
                writer.WriteEndArray();
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
                        metadata = await MetadataFormatter.Deserialize(reader, currentPath + "/" + MetaKeyName);
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

                var resourceObject = await ResourceObjectFormatter.Deserialize(reader, currentPath + "/" + index);
                primaryData.Add(resourceObject);

                index++;
            }

            return primaryData.ToArray();
        }
    }
}