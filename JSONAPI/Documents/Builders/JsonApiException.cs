using System;
using System.Net;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Exception that should be thrown by document builders if an error occurs. The data in
    /// this exception will drive the construction of the error object in the response document,
    /// as well as the HTTP status code.
    /// </summary>
    public class JsonApiException : Exception
    {
        /// <summary>
        /// The error
        /// </summary>
        public IError Error { get; set; }

        /// <summary>
        /// Creates a new JsonApiException
        /// </summary>
        /// <param name="error"></param>
        public JsonApiException(IError error)
        {
            Error = error;
        }

        /// <summary>
        /// Creates a JsonApiException indicating a problem with a supplied query parameter
        /// </summary>
        public static JsonApiException CreateForParameterError(string title, string detail, string parameter)
        {
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Status = HttpStatusCode.BadRequest,
                Title = title,
                Detail = detail,
                Parameter = parameter
            };
            return new JsonApiException(error);
        }

        /// <summary>
        /// Creates a JsonApiException with a title and detail
        /// </summary>
        public static JsonApiException Create(string title, string detail, HttpStatusCode status)
        {
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Status = status,
                Title = title,
                Detail = detail
            };
            return new JsonApiException(error);
        }

        /// <summary>
        /// Creates a JsonApiException to send a 404 Not Found error.
        /// </summary>
        public static JsonApiException CreateForNotFound(string detail = null)
        {
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Status = HttpStatusCode.NotFound,
                Title = "Resource not found",
                Detail = detail
            };
            return new JsonApiException(error);
        }

        /// <summary>
        /// Creates a JsonApiException to send a 403 Forbidden error.
        /// </summary>
        public static JsonApiException CreateForForbidden(string detail = null)
        {
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Status = HttpStatusCode.Forbidden,
                Title = "Forbidden",
                Detail = detail
            };
            return new JsonApiException(error);
        }
    }
}
