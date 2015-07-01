using System.Net;

namespace JSONAPI.Documents
{
    /// <summary>
    /// Default implementation of IError
    /// </summary>
    public class Error : IError
    {
        public string Id { get; set; }
        public ILink AboutLink { get; set; }
        public HttpStatusCode Status { get; set; }
        public string Code { get; set; }
        public string Title { get; set; }
        public string Detail { get; set; }
        public string Pointer { get; set; }
        public string Parameter { get; set; }
        public IMetadata Metadata { get; set; }
    }
}