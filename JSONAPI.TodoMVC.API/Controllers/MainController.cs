using JSONAPI.Http;

namespace JSONAPI.TodoMVC.API.Controllers
{
    public class MainController : JsonApiController
    {
        public MainController(IDocumentMaterializerLocator documentMaterializerLocator)
            : base(documentMaterializerLocator)
        {
        }
    }
}