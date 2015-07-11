using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class BuildingsController : JsonApiController<Building>
    {
        public BuildingsController(IDocumentMaterializer<Building> documentMaterializer)
            : base(documentMaterializer)
        {
        }
    }
}