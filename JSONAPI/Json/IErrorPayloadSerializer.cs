using JSONAPI.Payload;

namespace JSONAPI.Json
{
    /// <summary>
    /// Service responsible for serializing IErrorPayload instances
    /// </summary>
    public interface IErrorPayloadSerializer : IJsonApiSerializer<IErrorPayload>
    {
    }
}