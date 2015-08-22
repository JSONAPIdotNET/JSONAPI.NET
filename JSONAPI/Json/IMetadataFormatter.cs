using JSONAPI.Documents;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IMetadata instances
    /// </summary>
    public interface IMetadataFormatter : IJsonApiFormatter<IMetadata>
    {
    }
}