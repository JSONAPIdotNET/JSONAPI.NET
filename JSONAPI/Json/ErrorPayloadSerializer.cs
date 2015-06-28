using System;
using System.Threading.Tasks;
using JSONAPI.Payload;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IErrorPayloadSerializer
    /// </summary>
    public class ErrorPayloadSerializer : IErrorPayloadSerializer
    {
        private readonly IErrorSerializer _errorSerializer;
        private readonly IMetadataSerializer _metadataSerializer;

        /// <summary>
        /// Creates a new ErrorPayloadSerializer
        /// </summary>
        /// <param name="errorSerializer"></param>
        /// <param name="metadataSerializer"></param>
        public ErrorPayloadSerializer(IErrorSerializer errorSerializer, IMetadataSerializer metadataSerializer)
        {
            _errorSerializer = errorSerializer;
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(IErrorPayload payload, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("errors");
            writer.WriteStartArray();
            foreach (var error in payload.Errors)
            {
                _errorSerializer.Serialize(error, writer);
            }
            writer.WriteEndArray();

            if (payload.Metadata != null)
            {
                writer.WritePropertyName("meta");
                _metadataSerializer.Serialize(payload.Metadata, writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        public Task<IErrorPayload> Deserialize(JsonReader reader, string currentPath)
        {
            // The client should never be sending us errors.
            throw new NotSupportedException();
        }
    }
}
