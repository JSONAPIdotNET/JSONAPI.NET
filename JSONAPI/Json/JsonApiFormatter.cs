using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// MediaTypeFormatter for JSON API
    /// </summary>
    public class JsonApiFormatter : JsonMediaTypeFormatter
    {
        private readonly ISingleResourceDocumentFormatter _singleResourceDocumentFormatter;
        private readonly IResourceCollectionDocumentFormatter _resourceCollectionDocumentFormatter;
        private readonly IErrorDocumentFormatter _errorDocumentFormatter;
        private readonly IErrorDocumentBuilder _errorDocumentBuilder;

        /// <summary>
        /// Creates a new JsonApiFormatter
        /// </summary>
        public JsonApiFormatter(ISingleResourceDocumentFormatter singleResourceDocumentFormatter,
            IResourceCollectionDocumentFormatter resourceCollectionDocumentFormatter,
            IErrorDocumentFormatter errorDocumentFormatter,
            IErrorDocumentBuilder errorDocumentBuilder)
        {
            _singleResourceDocumentFormatter = singleResourceDocumentFormatter;
            _resourceCollectionDocumentFormatter = resourceCollectionDocumentFormatter;
            _errorDocumentFormatter = errorDocumentFormatter;
            _errorDocumentBuilder = errorDocumentBuilder;

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

        public override async Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            if (type == typeof(IJsonApiDocument) && value == null)
                return;

            var contentHeaders = content == null ? null : content.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            var writer = CreateJsonWriter(typeof(object), writeStream, effectiveEncoding);

            var singleResourceDocument = value as ISingleResourceDocument;
            var resourceCollectionDocument = value as IResourceCollectionDocument;
            var errorDocument = value as IErrorDocument;
            if (singleResourceDocument != null)
            {
                await _singleResourceDocumentFormatter.Serialize(singleResourceDocument, writer);
            }
            else if (resourceCollectionDocument != null)
            {
                await _resourceCollectionDocumentFormatter.Serialize(resourceCollectionDocument, writer);
            }
            else if (errorDocument != null)
            {
                await _errorDocumentFormatter.Serialize(errorDocument, writer);
            }
            else
            {
                var error = value as HttpError;
                if (error != null)
                {
                    var httpErrorDocument = _errorDocumentBuilder.BuildFromHttpError(error, HttpStatusCode.InternalServerError);
                    await _errorDocumentFormatter.Serialize(httpErrorDocument, writer);
                }
                else
                {
                    WriteErrorForUnsupportedType(value, writer);
                }
            }

            writer.Flush();
        }

        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var contentHeaders = content == null ? null : content.Headers;
            var effectiveEncoding = SelectCharacterEncoding(contentHeaders);
            var reader = CreateJsonReader(typeof(IDictionary<string, object>), readStream,
                effectiveEncoding);
            reader.DateParseHandling = DateParseHandling.None; // We will decide whether to parse as a DateTime or DateTimeOffset according to the attribute type

            reader.Read();

            if (typeof(ISingleResourceDocument).IsAssignableFrom(type))
                return await _singleResourceDocumentFormatter.Deserialize(reader, "");
            if (typeof(IResourceCollectionDocument).IsAssignableFrom(type))
                return await _resourceCollectionDocumentFormatter.Deserialize(reader, "");

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
