using JSONAPI.Payload;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IError instances
    /// </summary>
    public interface IErrorSerializer : IJsonApiSerializer<IError>
    {
    }
}