using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Describes linkage to a single resource
    /// </summary>
    public class ToOneResourceLinkage : IResourceLinkage
    {
        public JToken LinkageToken { get; private set; }

        /// <summary>
        /// Creates a to-one resource linkage object
        /// </summary>
        /// <param name="resourceIdentifier"></param>
        public ToOneResourceLinkage(IResourceIdentifier resourceIdentifier)
        {
            if (resourceIdentifier != null)
            {
                LinkageToken = new JObject();

                LinkageToken["type"] = resourceIdentifier.Type;
                LinkageToken["id"] = resourceIdentifier.Id;
            }
        }
    }
}