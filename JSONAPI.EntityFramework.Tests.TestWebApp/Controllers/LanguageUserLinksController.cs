using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class LanguageUserLinksController : JsonApiController<LanguageUserLink>
    {
        public LanguageUserLinksController(IPayloadMaterializer<LanguageUserLink> payloadMaterializer)
            : base(payloadMaterializer)
        {
        }
    }
}