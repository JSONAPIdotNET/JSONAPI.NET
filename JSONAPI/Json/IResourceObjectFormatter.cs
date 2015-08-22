using JSONAPI.Documents;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IResourceObject instances
    /// </summary>
    public interface IResourceObjectFormatter : IJsonApiFormatter<IResourceObject>
    {
    }
}