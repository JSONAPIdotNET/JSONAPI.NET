using JSONAPI.Documents;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IRelationshipObject instances
    /// </summary>
    public interface IRelationshipObjectFormatter : IJsonApiFormatter<IRelationshipObject>
    {
    }
}