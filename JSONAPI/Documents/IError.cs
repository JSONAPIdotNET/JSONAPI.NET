using System.Net;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Represents an error appearing in the `errors` array at the document top-level
    /// </summary>
    public interface IError
    {
        /// <summary>
        /// a unique identifier for this particular occurrence of the problem. 
        /// </summary>
        string Id { get; }

        /// <summary>
        /// link to information about this error
        /// </summary>
        ILink AboutLink { get; }

        /// <summary>
        /// the HTTP status code applicable to this problem, expressed as a string value.
        /// </summary>
        HttpStatusCode Status { get; }

        /// <summary>
        /// an application-specific error code, expressed as a string value.
        /// </summary>
        string Code { get; }

        /// <summary>
        /// a short, human-readable summary of the problem that SHOULD NOT change from occurrence to occurrence of the problem, except for purposes of localization.
        /// </summary>
        string Title { get; }

        /// <summary>
        /// a human-readable explanation specific to this occurrence of the problem.
        /// </summary>
        string Detail { get; }

        /// <summary>
        /// a JSON Pointer [RFC6901] to the associated entity in the request document
        /// </summary>
        string Pointer { get; }

        /// <summary>
        /// a string indicating which query parameter caused the error.
        /// </summary>
        string Parameter { get; }

        /// <summary>
        /// a meta object containing non-standard meta-information about the error.
        /// </summary>
        IMetadata Metadata { get; }
    }
}