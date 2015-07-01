using System;
using System.Net;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IErrorFormatter
    /// </summary>
    public class ErrorFormatter : IErrorFormatter
    {
        private readonly ILinkFormatter _linkFormatter;
        private readonly IMetadataFormatter _metadataFormatter;

        /// <summary>
        /// Creates a new errorFormatter
        /// </summary>
        /// <param name="linkFormatter"></param>
        /// <param name="metadataFormatter"></param>
        public ErrorFormatter(ILinkFormatter linkFormatter, IMetadataFormatter metadataFormatter)
        {
            _linkFormatter = linkFormatter;
            _metadataFormatter = metadataFormatter;
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
                _linkFormatter.Serialize(error.AboutLink, writer);
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
                _metadataFormatter.Serialize(error.Metadata, writer);
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