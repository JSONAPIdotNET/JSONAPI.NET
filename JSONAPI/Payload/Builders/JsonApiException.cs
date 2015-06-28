using System;
using System.Net;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Exception that should be thrown by payload builders if an error occurs. The data in
    /// this exception will drive the construction of the error object in the response payload,
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
        /// <param name="title"></param>
        /// <param name="detail"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
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
    }
}
