using System;

namespace JSONAPI.Json
{
    /// <summary>
    /// An exception that may be thrown by document formatters during deserialization
    /// in response to a JSON API-noncompliant document being submitted by the client.
    /// </summary>
    public class DeserializationException : Exception
    {
        /// <summary>
        /// The title of the error
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The path in the document where the error occurred.
        /// </summary>
        public string Pointer { get; private set; }

        /// <summary>
        /// Creates a new DeserializationException
        /// </summary>
        /// <param name="title"></param>
        /// <param name="message"></param>
        /// <param name="pointer"></param>
        public DeserializationException(string title, string message, string pointer)
            : base(message)
        {
            Title = title;
            Pointer = pointer;
        }
    }
}
