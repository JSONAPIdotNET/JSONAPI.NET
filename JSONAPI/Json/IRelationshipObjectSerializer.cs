using JSONAPI.Payload;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IRelationshipObject instances
    /// </summary>
    public interface IRelationshipObjectSerializer : IJsonApiSerializer<IRelationshipObject>
    {
    }
}