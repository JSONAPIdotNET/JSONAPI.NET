using System;
using System.Threading.Tasks;
using JSONAPI.Documents;
using Newtonsoft.Json;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IErrorDocumentFormatter
    /// </summary>
    public class ErrorDocumentFormatter : IErrorDocumentFormatter
    {
        private readonly IErrorFormatter _errorFormatter;
        private readonly IMetadataFormatter _metadataFormatter;

        /// <summary>
        /// Creates a new ErrorDocumentFormatter
        /// </summary>
        /// <param name="errorFormatter"></param>
        /// <param name="metadataFormatter"></param>
        public ErrorDocumentFormatter(IErrorFormatter errorFormatter, IMetadataFormatter metadataFormatter)
        {
            _errorFormatter = errorFormatter;
            _metadataFormatter = metadataFormatter;
        }

        public Task Serialize(IErrorDocument document, JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("errors");
            writer.WriteStartArray();
            foreach (var error in document.Errors)
            {
                _errorFormatter.Serialize(error, writer);
            }
            writer.WriteEndArray();

            if (document.Metadata != null)
            {
                writer.WritePropertyName("meta");
                _metadataFormatter.Serialize(document.Metadata, writer);
            }

            writer.WriteEndObject();

            return Task.FromResult(0);
        }

        public Task<IErrorDocument> Deserialize(JsonReader reader, string currentPath)
        {
            // The client should never be sending us errors.
            throw new NotSupportedException();
        }
    }
}
