using JSONAPI.Documents;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IError instances
    /// </summary>
    public interface IErrorFormatter : IJsonApiFormatter<IError>
    {
    }
}