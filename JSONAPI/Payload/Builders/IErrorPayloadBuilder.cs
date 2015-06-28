using System;
using System.Net;
using System.Web.Http;

namespace JSONAPI.Payload.Builders
{
    /// <summary>
    /// Provides services for building an error payload
    /// </summary>
    public interface IErrorPayloadBuilder
    {
        /// <summary>
        /// Builds an error payload based on an exception
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        IErrorPayload BuildFromException(Exception exception);

        /// <summary>
        /// Builds an error payload based on an HttpError
        /// </summary>
        /// <param name="httpError"></param>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        IErrorPayload BuildFromHttpError(HttpError httpError, HttpStatusCode statusCode);
    }
}