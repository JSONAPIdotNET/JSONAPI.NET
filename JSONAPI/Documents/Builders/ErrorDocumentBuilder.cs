using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Http;
using JSONAPI.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Documents.Builders
{
    /// <summary>
    /// Default implementation of IErrorDocumentBuilder
    /// </summary>
    public class ErrorDocumentBuilder : IErrorDocumentBuilder
    {
        private readonly IDictionary<Type, Func<Exception, IError>> _specificExceptionHandlers;

        /// <summary>
        /// Creates a new ErrorDocumentBuilder
        /// </summary>
        public ErrorDocumentBuilder()
        {
            _specificExceptionHandlers = new Dictionary<Type, Func<Exception, IError>>();
            _specificExceptionHandlers[typeof(JsonApiException)] = GetErrorForJsonApiException;
            _specificExceptionHandlers[typeof(DeserializationException)] = GetErrorForDeserializationException;
        }

        public IErrorDocument BuildFromException(Exception exception)
        {
            var error = BuildErrorForException(exception);

            var topLevelMetadata = GetTopLevelMetadata();
            return new ErrorDocument(new [] { error }, topLevelMetadata);
        }

        public IErrorDocument BuildFromHttpError(HttpError httpError, HttpStatusCode statusCode)
        {
            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Title = "An HttpError was returned.",
                Detail = httpError.Message,
                Status = statusCode,
                Metadata = GetHttpErrorMetadata(httpError)
            };


            var topLevelMetadata = GetTopLevelMetadata();
            return new ErrorDocument(new[] { (IError)error }, topLevelMetadata);
        }

        /// <summary>
        /// Builds an error object for a given exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual IError BuildErrorForException(Exception exception)
        {
            foreach (var specificExceptionHandler in _specificExceptionHandlers)
            {
                if (specificExceptionHandler.Key.IsInstanceOfType(exception))
                    return specificExceptionHandler.Value(exception);
            }

            var error = new Error
            {
                Id = Guid.NewGuid().ToString(),
                Title = "Unhandled exception",
                Detail = "An unhandled exception was thrown while processing the request.",
                AboutLink = GetAboutLinkForException(exception),
                Status = HttpStatusCode.InternalServerError,
                Metadata = GetErrorMetadata(exception)
            };
            return error;
        }

        /// <summary>
        /// Gets metadata to serialize inside the error object for a given exception.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual IMetadata GetErrorMetadata(Exception exception)
        {
            return new ExceptionErrorMetadata(exception);
        }

        /// <summary>
        /// Gets a link to an about resource that yields further details about this particular occurrence of the problem.
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        protected virtual ILink GetAboutLinkForException(Exception exception)
        {
            return null;
        }

        /// <summary>
        /// Allows configuring top-level metadata for an error response document.
        /// </summary>
        /// <returns></returns>
        protected virtual IMetadata GetTopLevelMetadata()
        {
            return null;
        }

        /// <summary>
        /// Gets metadata for an HttpError
        /// </summary>
        /// <param name="httpError"></param>
        /// <returns></returns>
        protected virtual IMetadata GetHttpErrorMetadata(HttpError httpError)
        {
            var metaObject = new JObject();

            var currentObject = metaObject;
            var currentError = httpError;
            while (currentError != null)
            {
                currentObject["exceptionType"] = currentError.ExceptionType;
                currentObject["exceptionMessage"] = currentError.ExceptionMessage;
                currentObject["stackTrace"] = currentError.StackTrace;

                currentError = currentError.InnerException;

                if (currentError != null)
                {
                    var innerObject = new JObject();
                    currentObject["innerException"] = innerObject;
                    currentObject = innerObject;
                }
            }

            return new BasicMetadata(metaObject);
        }

        private IError GetErrorForJsonApiException(Exception ex)
        {
            return ((JsonApiException) ex).Error;
        }

        private IError GetErrorForDeserializationException(Exception ex)
        {
            var deserializationException = (DeserializationException) ex;
            return new Error
            {
                Id = Guid.NewGuid().ToString(),
                Title = deserializationException.Title,
                Detail = ex.Message,
                Pointer = deserializationException.Pointer,
                Status = HttpStatusCode.BadRequest
            };
        }
    }
}
