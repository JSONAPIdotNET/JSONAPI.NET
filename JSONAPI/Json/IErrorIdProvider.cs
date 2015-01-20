using System.Web.Http;

namespace JSONAPI.Json
{
    internal interface IErrorIdProvider
    {
        string GenerateId(HttpError error);
    }
}
