namespace JSONAPI.Payload
{
    /// <summary>
    /// Base interface for payloads
    /// </summary>
    public interface IJsonApiPayload
    {
        /// <summary>
        /// Metadata for the payload as a whole
        /// </summary>
        IMetadata Metadata { get; }
    }
}