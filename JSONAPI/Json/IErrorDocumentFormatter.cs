using JSONAPI.Documents;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IErrorDocument instances
    /// </summary>
    public interface IErrorDocumentFormatter : IJsonApiFormatter<IErrorDocument>
    {
    }
}