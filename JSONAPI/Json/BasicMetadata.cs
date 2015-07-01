using JSONAPI.Documents;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Json
{
    /// <summary>
    /// Default implementation of IMetadata
    /// </summary>
    public class BasicMetadata : IMetadata
    {
        /// <summary>
        /// Creates a new BasicMetadata
        /// </summary>
        /// <param name="metaObject"></param>
        public BasicMetadata(JObject metaObject)
        {
            MetaObject = metaObject;
        }

        public JObject MetaObject { get; private set; }
    }
}