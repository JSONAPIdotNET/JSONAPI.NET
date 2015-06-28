using Newtonsoft.Json.Linq;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Describes a relationship's linkage
    /// </summary>
    public interface IResourceLinkage
    {
        /// <summary>
        /// The item determining the linkage 
        /// </summary>
        JToken LinkageToken { get; }
    }
}