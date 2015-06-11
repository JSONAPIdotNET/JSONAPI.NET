using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IPayload
    /// </summary>
    public class Payload : IPayload
    {
        public object PrimaryData { get; set; }
        public JObject Metadata { get; set; }
    }
}
