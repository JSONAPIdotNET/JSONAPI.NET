using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IResourceObjectFormatter
    /// </summary>
    public class ResourceObjectFormatter : IResourceObjectFormatter
    {
        private IRelationshipObjectFormatter _relationshipObjectFormatter;
        private ILinkFormatter _linkFormatter;
        private IMetadataFormatter _metadataFormatter;
        private const string TypeKeyName = "type";
        private const string IdKeyName = "id";
        private const string AttributesKeyName = "attributes";
        private const string RelationshipsKeyName = "relationships";
        private const string MetaKeyName = "meta";
        private const string LinksKeyName = "links";
        private const string SelfLinkKeyName = "self";

        /// <summary>
        /// Constructs a new ResourceObjectFormatter
        /// </summary>
        public ResourceObjectFormatter()
        {
            
        }

        /// <summary>
        /// Constructs a new ResourceObjectFormatter
        /// </summary>
        /// <param name="relationshipObjectFormatter">The formatter to use for relationship objects</param>
        /// <param name="linkFormatter">The formatter to use for links</param>
        /// <param name="metadataFormatter">The formatter to use for metadata</param>
        public ResourceObjectFormatter(IRelationshipObjectFormatter relationshipObjectFormatter, ILinkFormatter linkFormatter, IMetadataFormatter metadataFormatter)
        {
            _relationshipObjectFormatter = relationshipObjectFormatter;
            _linkFormatter = linkFormatter;
            _metadataFormatter = metadataFormatter;
        }

        /// <summary>
        /// The formatter to use for relationship objects belonging to this resource
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public IRelationshipObjectFormatter RelationshipObjectFormatter
        {
            get
            {
                return _relationshipObjectFormatter ?? (_relationshipObjectFormatter = new RelationshipObjectFormatter());
            }
            set
            {
                if (_relationshipObjectFormatter != null) throw new InvalidOperationException("This property can only be set once.");
                _relationshipObjectFormatter = value;
            }
        }

        /// <summary>
        /// The formatter to use for links
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public ILinkFormatter LinkFormatter
        {
            get
            {
                return _linkFormatter ?? (_linkFormatter = new LinkFormatter());
            }
            set
            {
                if (_linkFormatter != null) throw new InvalidOperationException("This property can only be set once.");
                _linkFormatter = value;
            }
        }

        /// <summary>
        /// The formatter to use for metadata that belongs to this resource object
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

        public Task Serialize(IResourceObject resourceObject, JsonWriter writer)
        {
            if (resourceObject == null)
            {
                writer.WriteNull();
                return Task.FromResult(0);
            }

            writer.WriteStartObject();

            writer.WritePropertyName(TypeKeyName);
            writer.WriteValue(resourceObject.Type);
            writer.WritePropertyName(IdKeyName);
            writer.WriteValue(resourceObject.Id);

            if (resourceObject.Attributes != null && resourceObject.Attributes.Any())
            {
                writer.WritePropertyName(AttributesKeyName);
                writer.WriteStartObject();
                foreach (var attribute in resourceObject.Attributes)
                {
                    writer.WritePropertyName(attribute.Key);
                    if (attribute.Value == null)
                        writer.WriteNull();
                    else
                    {
                        if (attribute.Value.Type == JTokenType.Integer)
                        {
                            var ulongValue = attribute.Value.Value<ulong>();
                            if (ulongValue > long.MaxValue)
                            {
                                writer.WriteRawValue(ulongValue.ToString());
                                continue;
                            }
                        }
                        attribute.Value.WriteTo(writer);
                    }
                }
                writer.WriteEndObject();
            }

            if (resourceObject.Relationships != null)
            {
                var relationshipsToRender = resourceObject.Relationships.Where(r => r.Value != null).ToArray();
                if (relationshipsToRender.Any())
                {
                    writer.WritePropertyName(RelationshipsKeyName);
                    writer.WriteStartObject();
                    foreach (var relationship in relationshipsToRender)
                    {
                        if (relationship.Value == null) continue;
                        writer.WritePropertyName(relationship.Key);
                        RelationshipObjectFormatter.Serialize(relationship.Value, writer);
                    }
                    writer.WriteEndObject();
                }
            }

            if (resourceObject.SelfLink != null)
            {
                writer.WritePropertyName(LinksKeyName);
                writer.WriteStartObject();
                writer.WritePropertyName(SelfLinkKeyName);
                LinkFormatter.Serialize(resourceObject.SelfLink, writer);
                writer.WriteEndObject();
            }

            if (resourceObject.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                MetadataFormatter.Serialize(resourceObject.Metadata, writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        public async Task<IResourceObject> Deserialize(JsonReader reader, string currentPath)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new DeserializationException("Invalid resource object", "Expected an object.", currentPath);

            string type = null;
            string id = null;
            IMetadata metadata = null;
            IDictionary<string, JToken> attributes = null;
            IDictionary<string, IRelationshipObject> relationships = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                var propertyName = (string)reader.Value;
                reader.Read();

                switch (propertyName)
                {
                    case TypeKeyName:
                        type = (string) reader.Value;
                        break;
                    case IdKeyName:
                        id = (string) reader.Value;
                        break;
                    case MetaKeyName:
                        metadata = await MetadataFormatter.Deserialize(reader, currentPath + "/" + MetaKeyName);
                        break;
                    case AttributesKeyName:
                        attributes = DeserializeAttributes(reader, currentPath + "/" + AttributesKeyName);
                        break;
                    case RelationshipsKeyName:
                        relationships = await DeserializeRelationships(reader, currentPath + "/" + RelationshipsKeyName);
                        break;
                    default:
                        reader.Skip();
                        break;
                }
            }

            if (string.IsNullOrEmpty(type))
                throw new DeserializationException("Resource object missing type", "Expected a value for `type`", currentPath + "/type");

            return new ResourceObject(type, id,
                attributes ?? new Dictionary<string, JToken>(),
                relationships ?? new Dictionary<string, IRelationshipObject>(),
                metadata: metadata);
        }

        private IDictionary<string, JToken> DeserializeAttributes(JsonReader reader, string currentPath)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new DeserializationException("Invalid attributes object", "Expected an object but encountered " + reader.TokenType, currentPath);

            var attributes = new Dictionary<string, JToken>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                var attributeName = (string)reader.Value;
                reader.Read();

                var attributeValue = JToken.ReadFrom(reader);
                attributes.Add(attributeName, attributeValue);
            }

            return attributes;
        }

        private async Task<IDictionary<string, IRelationshipObject>> DeserializeRelationships(JsonReader reader, string currentPath)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new DeserializationException("Invalid relationship object", "Expected an object but encountered " + reader.TokenType, currentPath);

            var relationships = new Dictionary<string, IRelationshipObject>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                var relationshipName = (string)reader.Value;
                reader.Read();

                var relationship = await RelationshipObjectFormatter.Deserialize(reader, currentPath + "/" + relationshipName);
                relationships.Add(relationshipName, relationship);
            }

            return relationships;
        }
    }
}