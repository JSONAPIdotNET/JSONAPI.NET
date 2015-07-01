using System;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Metadata object for serializing exceptions in a response
    /// </summary>
    public class ExceptionErrorMetadata : IMetadata
    {
        /// <summary>
        /// Creates a new ExceptionErrorMetadata
        /// </summary>
        /// <param name="exception"></param>
        public ExceptionErrorMetadata(Exception exception)
        {
            MetaObject = new JObject();

            var currentObject = MetaObject;
            var currentException = exception;
            while (currentException != null)
            {
                currentObject["exceptionMessage"] = currentException.Message;
                currentObject["stackTrace"] = currentException.StackTrace;

                currentException = currentException.InnerException;

                if (currentException != null)
                {
                    var innerObject = new JObject();
                    currentObject["innerException"] = innerObject;
                    currentObject = innerObject;
                }
            }
        }

        public JObject MetaObject { get; private set; }
    }
}