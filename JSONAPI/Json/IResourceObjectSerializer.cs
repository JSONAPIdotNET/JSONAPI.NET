using JSONAPI.Payload;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IResourceObject instances
    /// </summary>
    public interface IResourceObjectSerializer : IJsonApiSerializer<IResourceObject>
    {
    }
}