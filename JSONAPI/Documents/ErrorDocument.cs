namespace JSONAPI.Documents
{
    /// <summary>
    /// Default implementation of IErrorDocument
    /// </summary>
    public class ErrorDocument : IErrorDocument
    {
        public IError[] Errors { get; private set; }
        public IMetadata Metadata { get; private set; }

        /// <summary>
        /// Creates a new ErrorDocument
        /// </summary>
        /// <param name="errors"></param>
        /// <param name="metadata"></param>
        public ErrorDocument(IError[] errors, IMetadata metadata)
        {
            Errors = errors;
            Metadata = metadata;
        }
    }
}