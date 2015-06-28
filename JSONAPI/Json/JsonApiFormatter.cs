using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// MediaTypeFormatter for JSON API
    /// </summary>
    public class JsonApiFormatter : JsonMediaTypeFormatter
    {
        private readonly ISingleResourcePayloadSerializer _singleResourcePayloadSerializer;
        private readonly IResourceCollectionPayloadSerializer _resourceCollectionPayloadSerializer;
        private readonly IErrorPayloadSerializer _errorPayloadSerializer;
        private readonly IErrorPayloadBuilder _errorPayloadBuilder;

        /// <summary>
        /// Creates a new JsonApiFormatter
        /// </summary>
        public JsonApiFormatter(ISingleResourcePayloadSerializer singleResourcePayloadSerializer,
            IResourceCollectionPayloadSerializer resourceCollectionPayloadSerializer,
            IErrorPayloadSerializer errorPayloadSerializer,
            IErrorPayloadBuilder errorPayloadBuilder)
        {
            _singleResourcePayloadSerializer = singleResourcePayloadSerializer;
            _resourceCollectionPayloadSerializer = resourceCollectionPayloadSerializer;
            _errorPayloadSerializer = errorPayloadSerializer;
            _errorPayloadBuilder = errorPayloadBuilder;

            SupportedMediaTypes.Clear();
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/vnd.api+json"));
        }

        public override bool CanReadType(Type t)
        {
            return true;
        }

        public override bool CanWriteType(Type t)
        {
            return true;
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == typeof(IJsonApiPayload) && value == null)
                return Task.FromResult(0);

            var contentHeaders = content == null ? null : content.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            var writer = CreateJsonWriter(typeof(object), writeStream, effectiveEncoding);

            var singleResourcePayload = value as ISingleResourcePayload;
            var resourceCollectionPayload = value as IResourceCollectionPayload;
            var errorPayload = value as IErrorPayload;
            if (singleResourcePayload != null)
            {
                _singleResourcePayloadSerializer.Serialize(singleResourcePayload, writer);
            }
            else if (resourceCollectionPayload != null)
            {
                _resourceCollectionPayloadSerializer.Serialize(resourceCollectionPayload, writer);
            }
            else if (errorPayload != null)
            {
                _errorPayloadSerializer.Serialize(errorPayload, writer);
            }
            else
            {
                var error = value as HttpError;
                if (error != null)
                {
                    var httpErrorPayload = _errorPayloadBuilder.BuildFromHttpError(error, HttpStatusCode.InternalServerError);
                    _errorPayloadSerializer.Serialize(httpErrorPayload, writer);
                }
                else
                {
                    WriteErrorForUnsupportedType(value, writer);
                }
            }

            writer.Flush();

            return Task.FromResult(0);
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var contentHeaders = content == null ? null : content.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            var reader = CreateJsonReader(typeof(IDictionary<string, object>), readStream,
                effectiveEncoding);
            reader.DateParseHandling = DateParseHandling.None; // We will decide whether to parse as a DateTime or DateTimeOffset according to the attribute type

            reader.Read();

            if (typeof(ISingleResourcePayload).IsAssignableFrom(type))
                return await _singleResourcePayloadSerializer.Deserialize(reader, "");
            if (typeof(IResourceCollectionPayload).IsAssignableFrom(type))
                return await _resourceCollectionPayloadSerializer.Deserialize(reader, "");

            throw new Exception(string.Format("The type {0} is not supported for deserialization.", type.Name));
        }

        private void WriteErrorForUnsupportedType(object obj, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("title");
            writer.WriteValue("Unexpected Content");
            writer.WritePropertyName("detail");
            writer.WriteValue("The JsonApiFormatter was asked to serialize an unsupported object.");
            writer.WritePropertyName("meta");
            writer.WriteStartObject();
            writer.WritePropertyName("objectTypeName");
            writer.WriteValue(obj.GetType().Name);
            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
