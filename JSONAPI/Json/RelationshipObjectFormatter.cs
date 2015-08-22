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

        private readonly ILinkFormatter _linkFormatter;
        private readonly IResourceLinkageFormatter _resourceLinkageFormatter;
        private readonly IMetadataFormatter _metadataFormatter;

        /// <summary>
        /// Creates a new RelationshipObjectFormatter
        /// </summary>
        public RelationshipObjectFormatter(ILinkFormatter linkFormatter, IResourceLinkageFormatter resourceLinkageFormatter, IMetadataFormatter metadataFormatter)
        {
            _linkFormatter = linkFormatter;
            _resourceLinkageFormatter = resourceLinkageFormatter;
            _metadataFormatter = metadataFormatter;
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
                    _linkFormatter.Serialize(relationshipObject.SelfLink, writer);
                }
                if (relationshipObject.RelatedResourceLink != null)
                {
                    writer.WritePropertyName(RelatedLinkKeyName);
                    _linkFormatter.Serialize(relationshipObject.RelatedResourceLink, writer);
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
                _resourceLinkageFormatter.Serialize(relationshipObject.Linkage, writer);
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
                _metadataFormatter.Serialize(relationshipObject.Metadata, writer);
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
                        linkage = await _resourceLinkageFormatter.Deserialize(reader, currentPath + "/" + LinkageKeyName);
                        break;
                    case MetaKeyName:
                        metadata = await _metadataFormatter.Deserialize(reader, currentPath + "/" + MetaKeyName);
                        break;
                }
            }

            return new RelationshipObject(linkage, metadata);
        }
    }
}