using Newtonsoft.Json.Linq;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Represents metadata that can be embedded in several places
    /// </summary>
    public interface IMetadata
    {
        /// <summary>
        /// A JSON object containing the metadata value
        /// </summary>
        JObject MetaObject { get; }
    }
}