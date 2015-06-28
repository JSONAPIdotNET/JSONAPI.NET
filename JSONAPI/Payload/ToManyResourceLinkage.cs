using System;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Describes linkage to a collection of resources
    /// </summary>
    public class ToManyResourceLinkage : IResourceLinkage
    {
        public JToken LinkageToken { get; private set; }

        /// <summary>
        /// Creates a To-many resource linkage object
        /// </summary>
        /// <param name="resourceIdentifiers"></param>
        /// <exception cref="NotImplementedException"></exception>
        public ToManyResourceLinkage(IResourceIdentifier[] resourceIdentifiers)
        {
            var array = new JArray();
            if (resourceIdentifiers != null)
            {
                foreach (var resourceIdentifier in resourceIdentifiers)
                {
                    var obj = new JObject();
                    obj["type"] = resourceIdentifier.Type;
                    obj["id"] = resourceIdentifier.Id;
                    array.Add(obj);
                }
            }
            LinkageToken = array;
        }
    }
}