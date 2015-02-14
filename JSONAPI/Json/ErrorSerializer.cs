using System;
using System.IO;
using System.Web.Http;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    internal class ErrorSerializer : IErrorSerializer
    {
        private class JsonApiError
        {
            [JsonProperty(PropertyName = "id")]
            public string Id { get; private set; }

            [JsonProperty(PropertyName = "status")]
            public string Status { get; private set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; private set; }

            [JsonProperty(PropertyName = "detail")]
            public string Detail { get; private set; }

            [JsonProperty(PropertyName = "stackTrace")]
            public string StackTrace { get; private set; }

            [JsonProperty(PropertyName = "inner")]
            public JsonApiError Inner { get; private set; }

            public JsonApiError(HttpError error, IErrorIdProvider idProvider)
            {
                Id = idProvider.GenerateId(error);
                Title = error.ExceptionType ?? error.Message;
                Status = "500";
                Detail = error.ExceptionMessage ?? error.MessageDetail;
                StackTrace = error.StackTrace;

                if (error.InnerException != null)
                    Inner = new JsonApiError(error.InnerException, idProvider);
            }
        }

        private readonly IErrorIdProvider _errorIdProvider;

        public ErrorSerializer()
            : this(new GuidErrorIdProvider())
        {
            
        }

        public ErrorSerializer(IErrorIdProvider errorIdProvider)
        {
            _errorIdProvider = errorIdProvider;
        }

        public bool CanSerialize(Type type)
        {
            return type == typeof (HttpError);
        }

        public void SerializeError(object error, Stream writeStream, JsonWriter writer, JsonSerializer serializer)
        {
            var httpError = error as HttpError;
            if (httpError == null) throw new Exception("Unsupported error type.");

            writer.WriteStartObject();
            writer.WritePropertyName("errors");

            var jsonApiError = new JsonApiError(httpError, _errorIdProvider);
            serializer.Serialize(writer, new[] { jsonApiError });

            writer.WriteEndObject();
        }
    }
}
