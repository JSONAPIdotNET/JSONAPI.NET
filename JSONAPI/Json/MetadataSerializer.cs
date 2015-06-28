using System.Threading.Tasks;
using JSONAPI.Payload;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IMetadataSerializer
    /// </summary>
    public class MetadataSerializer : IMetadataSerializer
    {
        public Task Serialize(IMetadata metadata, JsonWriter writer)
        {
            if (metadata == null)
            {
                writer.WriteNull();
            }
            else
            {
                if (metadata.MetaObject == null)
                    throw new JsonSerializationException("The meta object cannot be null.");
                metadata.MetaObject.WriteTo(writer);
            }
            return Task.FromResult(0);
        }

        public Task<IMetadata> Deserialize(JsonReader reader, string currentPath)
        {
            IMetadata metadata;
            if (reader.TokenType == JsonToken.Null)
            {
                metadata = null;
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = (JObject)JToken.ReadFrom(reader);
                metadata = new BasicMetadata(obj);
            }
            else
            {
                throw new DeserializationException("", "Expected an object or null for `meta`, but got " + reader.TokenType, currentPath);
            }

            return Task.FromResult(metadata);
        }
    }
}