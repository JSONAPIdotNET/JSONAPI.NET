using JSONAPI.Http;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Controllers
{
    public class MainController : JsonApiController
    {
        public MainController(IDocumentMaterializerLocator documentMaterializerLocator)
            : base(documentMaterializerLocator)
        {
        }
    }
}