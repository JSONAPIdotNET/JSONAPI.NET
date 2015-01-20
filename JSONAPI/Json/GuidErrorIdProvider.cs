using System;
using System.Web.Http;

namespace JSONAPI.Json
{
    internal class GuidErrorIdProvider : IErrorIdProvider
    {
        public string GenerateId(HttpError error)
        {
            return Guid.NewGuid().ToString();
        }
    }
}
