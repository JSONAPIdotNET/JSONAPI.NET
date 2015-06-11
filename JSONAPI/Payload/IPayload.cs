using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Actions may return objects that implement this interface in order to
    /// add metadata to the response. 
    /// </summary>
    public interface IPayload
    {
        /// <summary>
        /// The primary data of the response
        /// </summary>
        object PrimaryData { get; }

        /// <summary>
        /// Metadata to serialize at the top-level of the response
        /// </summary>
        JObject Metadata { get; }
    }
}
