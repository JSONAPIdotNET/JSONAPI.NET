using System;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IRelationshipObjectFormatter
    /// </summary>
    public class RelationshipObjectFormatter : IRelationshipObjectFormatter
    {
        private const string LinksKeyName = "links";
        private const string SelfLinkKeyName = "self";
        private const string RelatedLinkKeyName = "related";
        private const string LinkageKeyName = "data";
        private const string MetaKeyName = "meta";

        private ILinkFormatter _linkFormatter;
        private IResourceLinkageFormatter _resourceLinkageFormatter;
        private IMetadataFormatter _metadataFormatter;

        /// <summary>
        /// Creates a new RelationshipObjectFormatter
        /// </summary>
        public RelationshipObjectFormatter ()
        {
            
        }

        /// <summary>
        /// Creates a new RelationshipObjectFormatter
        /// </summary>
        public RelationshipObjectFormatter(ILinkFormatter linkFormatter, IResourceLinkageFormatter resourceLinkageFormatter, IMetadataFormatter metadataFormatter)
        {
            _linkFormatter = linkFormatter;
            _resourceLinkageFormatter = resourceLinkageFormatter;
            _metadataFormatter = metadataFormatter;
        }

        private ILinkFormatter LinkFormatter
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

        private IResourceLinkageFormatter ResourceLinkageFormatter
        {
            get
            {
                return _resourceLinkageFormatter ?? (_resourceLinkageFormatter = new ResourceLinkageFormatter());
            }
            set
            {
                if (_resourceLinkageFormatter != null) throw new InvalidOperationException("This property can only be set once.");
                _resourceLinkageFormatter = value;
            }
        }

        private IMetadataFormatter MetadataFormatter
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

        public Task Serialize(IRelationshipObject relationshipObject, JsonWriter writer)
        {
            if (relationshipObject.Metadata == null && relationshipObject.SelfLink == null &&
                relationshipObject.RelatedResourceLink == null && relationshipObject.Linkage == null)
                throw new JsonSerializationException(
                    String.Format("At least one of `{0}`, `{1}`, or `{2}` must be present in a relationship object.",
                        LinksKeyName, LinkageKeyName, MetaKeyName));

            writer.WriteStartObject();
            SerializeLinks(relationshipObject, writer);
            SerializeLinkage(relationshipObject, writer);
            SerializeMetadata(relationshipObject, writer);
            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        /// <summary>
        /// Serializes the relationship object's links.
        /// </summary>
        protected virtual void SerializeLinks(IRelationshipObject relationshipObject, JsonWriter writer)
        {
            if (relationshipObject.SelfLink != null || relationshipObject.RelatedResourceLink != null)
            {
                writer.WritePropertyName(LinksKeyName);
                writer.WriteStartObject();

                if (relationshipObject.SelfLink != null)
                {
                    writer.WritePropertyName(SelfLinkKeyName);
                    LinkFormatter.Serialize(relationshipObject.SelfLink, writer);
                }
                if (relationshipObject.RelatedResourceLink != null)
                {
                    writer.WritePropertyName(RelatedLinkKeyName);
                    LinkFormatter.Serialize(relationshipObject.RelatedResourceLink, writer);
                }

                writer.WriteEndObject();
            }
        }

        /// <summary>
        /// Serializes the relationship object's linkage.
        /// </summary>
        protected virtual void SerializeLinkage(IRelationshipObject relationshipObject, JsonWriter writer)
        {
            if (relationshipObject.Linkage != null)
            {
                writer.WritePropertyName(LinkageKeyName);
                ResourceLinkageFormatter.Serialize(relationshipObject.Linkage, writer);
            }
        }

        /// <summary>
        /// Serializes the relationship object's metadata.
        /// </summary>
        protected virtual void SerializeMetadata(IRelationshipObject relationshipObject, JsonWriter writer)
        {
            if (relationshipObject.Metadata != null)
            {
                writer.WritePropertyName(MetaKeyName);
                MetadataFormatter.Serialize(relationshipObject.Metadata, writer);
            }
        }

        public async Task<IRelationshipObject> Deserialize(JsonReader reader, string currentPath)
        {
            if (reader.TokenType != JsonToken.StartObject)
                throw new DeserializationException("Invalid relationship object", "Expected an object, but found " + reader.TokenType, currentPath);

            IResourceLinkage linkage = null;
            IMetadata metadata = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.EndObject)
                    break;

                var propertyName = (string)reader.Value;
                reader.Read();
                switch (propertyName)
                {
                    case LinkageKeyName:
                        linkage = await ResourceLinkageFormatter.Deserialize(reader, currentPath + "/" + LinkageKeyName);
                        break;
                    case MetaKeyName:
                        metadata = await MetadataFormatter.Deserialize(reader, currentPath + "/" + MetaKeyName);
                        break;
                }
            }

            return new RelationshipObject(linkage, metadata);
        }
    }
}