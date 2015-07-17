using System.Linq;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IResourceLinkageFormatter
    /// </summary>
    public class ResourceLinkageFormatter : IResourceLinkageFormatter
    {
        public Task Serialize(IResourceLinkage linkage, JsonWriter writer)
        {
            if (linkage.IsToMany)
            {
                writer.WriteStartArray();
                foreach (var resourceIdentifier in linkage.Identifiers)
                {
                    WriteResourceIdentifier(resourceIdentifier, writer);
                }
                writer.WriteEndArray();
            }
            else
            {
                var initialIdentifier = linkage.Identifiers.FirstOrDefault();
                if (initialIdentifier == null)
                    writer.WriteNull();
                else
                {
                    WriteResourceIdentifier(initialIdentifier, writer);
                }

            }
            return Task.FromResult(0);
        }

        private void WriteResourceIdentifier(IResourceIdentifier resourceIdentifier, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("type");
            writer.WriteValue(resourceIdentifier.Type);
            writer.WritePropertyName("id");
            writer.WriteValue(resourceIdentifier.Id);
            writer.WriteEndObject();
        }

        public Task<IResourceLinkage> Deserialize(JsonReader reader, string currentPath)
        {
            IResourceLinkage linkage;
            if (reader.TokenType == JsonToken.StartArray)
            {
                var array = JToken.ReadFrom(reader);
                var resourceIdentifiers = array.Select(t => ReadResourceIdentifier(t, currentPath)).ToArray();

                linkage = new ToManyResourceLinkage(resourceIdentifiers);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                var obj = JToken.ReadFrom(reader);
                var resourceIdentifier = ReadResourceIdentifier(obj, currentPath);

                linkage = new ToOneResourceLinkage(resourceIdentifier);
            }
            else if (reader.TokenType == JsonToken.Null)
            {
                linkage = new ToOneResourceLinkage(null);
            }
            else
            {
                throw new DeserializationException("Invalid linkage for relationship",
                    "Expected an array, object, or null for linkage, but got " + reader.TokenType, currentPath);
            }

            return Task.FromResult(linkage);
        }

        private IResourceIdentifier ReadResourceIdentifier(JToken t, string currentPath)
        {
            var jobject = t as JObject;
            if (jobject == null)
                throw new DeserializationException("Invalid resource identifier", "Resource identifiers must be an object.", currentPath);

            var typeToken = jobject["type"];
            if (typeToken == null)
                throw new DeserializationException("Resource identifier missing type", "The `type` key is missing.", currentPath);
            if (typeToken.Type != JTokenType.String)
                throw new DeserializationException("Resource identifier type invalid",
                    "Expected a string value for `type` but encountered " + typeToken.Type, currentPath + "/type");

            var type = typeToken.Value<string>();
            if (string.IsNullOrWhiteSpace(type))
                throw new DeserializationException("Resource identifier type empty", "The `type` key cannot be empty or whitespace.", currentPath + "/type");

            var idToken = jobject["id"];
            if (idToken == null)
                throw new DeserializationException("Resource identifier missing id", "The `id` key is missing.", currentPath);
            if (idToken.Type != JTokenType.String)
                throw new DeserializationException("Resource identifier id invalid",
                    "Expected a string value for `id` but encountered " + idToken.Type, currentPath + "/id");

            var id = idToken.Value<string>();
            if (string.IsNullOrWhiteSpace(id))
                throw new DeserializationException("Resource identifier id empty", "The `id` key cannot be empty or whitespace.", currentPath + "/id");

            return new ResourceIdentifier(type, id);
        }
    }
}