using System;
using System.Net;
using System.Web.Http;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Provides services for building an error document
    /// </summary>
    public interface IErrorDocumentBuilder
    {
        /// <summary>
        /// Builds an error document based on an exception
        /// </summary>
        IErrorDocument BuildFromException(Exception exception);

        /// <summary>
        /// Builds an error document based on an HttpError
        /// </summary>
        IErrorDocument BuildFromHttpError(HttpError httpError, HttpStatusCode statusCode);
    }
}