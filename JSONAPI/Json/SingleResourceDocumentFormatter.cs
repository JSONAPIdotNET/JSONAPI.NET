using System;
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
        private IResourceObjectFormatter _resourceObjectFormatter;
        private IMetadataFormatter _metadataFormatter;
        private const string PrimaryDataKeyName = "data";
        private const string RelatedDataKeyName = "included";
        private const string MetaKeyName = "meta";

        /// <summary>
        /// Creates a SingleResourceDocumentFormatter with default parameters
        /// </summary>
        public SingleResourceDocumentFormatter()
        {
        }

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

        public Task Serialize(ISingleResourceDocument document, JsonWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName(PrimaryDataKeyName);

            ResourceObjectFormatter.Serialize(document.PrimaryData, writer);

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

            if (document.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                MetadataFormatter.Serialize(document.Metadata, writer);
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
                        metadata = await MetadataFormatter.Deserialize(reader, currentPath + "/" + MetaKeyName);
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

            var primaryData = await ResourceObjectFormatter.Deserialize(reader, currentPath);
            return primaryData;
        }
    }
}