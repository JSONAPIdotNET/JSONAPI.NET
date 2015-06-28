namespace JSONAPI.Payload
{
    /// <summary>
    /// Default implementation of IErrorPayload
    /// </summary>
    public class ErrorPayload : IErrorPayload
    {
        public IError[] Errors { get; private set; }
        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Creates a new ErrorPayload
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="metadata"></param>
        public ErrorPayload(IError[] errors, IMetadata metadata)
        {
            Errors = errors;
            Metadata = metadata;
        }
    }
}