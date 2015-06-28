using System;
using System.Net;
using System.Threading.Tasks;
using JSONAPI.Payload;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IErrorSerializer
    /// </summary>
    public class ErrorSerializer : IErrorSerializer
    {
        private readonly ILinkSerializer _linkSerializer;
        private readonly IMetadataSerializer _metadataSerializer;

        /// <summary>
        /// Creates a new ErrorSerializer
        /// </summary>
        /// <param name="linkSerializer"></param>
        /// <param name="metadataSerializer"></param>
        public ErrorSerializer(ILinkSerializer linkSerializer, IMetadataSerializer metadataSerializer)
        {
            _linkSerializer = linkSerializer;
            _metadataSerializer = metadataSerializer;
        }

        public Task Serialize(IError error, JsonWriter writer)
        {
            writer.WriteStartObject();

            if (error.Id != null)
            {
                writer.WritePropertyName("id");
                writer.WriteValue(error.Id);
            }

            if (error.AboutLink != null)
            {
                writer.WritePropertyName("links");
                writer.WriteStartObject();
                writer.WritePropertyName("about");
                _linkSerializer.Serialize(error.AboutLink, writer);
                writer.WriteEndObject();
            }

            if (error.Status != default(HttpStatusCode))
            {
                writer.WritePropertyName("status");
                writer.WriteValue(((int)error.Status).ToString());
            }

            if (error.Code != null)
            {
                writer.WritePropertyName("code");
                writer.WriteValue(error.Code);
            }

            if (error.Title != null)
            {
                writer.WritePropertyName("title");
                writer.WriteValue(error.Title);
            }

            if (error.Detail != null)
            {
                writer.WritePropertyName("detail");
                writer.WriteValue(error.Detail);
            }

            if (error.Pointer != null || error.Parameter != null)
            {
                writer.WritePropertyName("source");
                writer.WriteStartObject();
                if (error.Pointer != null)
                {
                    writer.WritePropertyName("pointer");
                    writer.WriteValue(error.Pointer);
                }
                if (error.Parameter != null)
                {
                    writer.WritePropertyName("parameter");
                    writer.WriteValue(error.Parameter);
                }
                writer.WriteEndObject();
            }

            if (error.Metadata != null)
            {
                writer.WritePropertyName("meta");
                error.Metadata.MetaObject.WriteTo(writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        public Task<IError> Deserialize(JsonReader reader, string currentPath)
        {
            // The client should never be sending us errors
            throw new NotSupportedException();
        }
    }
}