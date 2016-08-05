using Newtonsoft.Json.Linq;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Default implementation of <see cref="IMetadata"/>
    /// </summary>
    public class Metadata : IMetadata
    {
        public Metadata()
        {
            MetaObject = new JObject();
        }
        public JObject MetaObject { get; }
    }
}
