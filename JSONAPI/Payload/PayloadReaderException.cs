using System;

namespace JSONAPI.Payload
{
    /// <summary>
    /// Exception thrown by an IPayloadReader when the payload is semantically incorrect.
    /// </summary>
    public class PayloadReaderException : Exception
    {
        /// <summary>
        /// Creates a new PayloadReaderException
        /// </summary>
        /// <param name="message"></param>
        public PayloadReaderException(string message)
            : base(message)
        {
            
        }
    }
}