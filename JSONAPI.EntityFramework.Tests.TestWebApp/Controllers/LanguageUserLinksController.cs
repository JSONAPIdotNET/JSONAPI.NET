using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class LanguageUserLinksController : JsonApiController<LanguageUserLink>
    {
        public LanguageUserLinksController(IDocumentMaterializer<LanguageUserLink> documentMaterializer)
            : base(documentMaterializer)
        {
        }
    }
}