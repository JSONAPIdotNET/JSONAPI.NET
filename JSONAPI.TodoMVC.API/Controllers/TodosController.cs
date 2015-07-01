using JSONAPI.Http;
using JSONAPI.TodoMVC.API.Models;

namespace JSONAPI.TodoMVC.API.Controllers
{
    public class TodosController : JsonApiController<Todo>
    {
        public TodosController(IDocumentMaterializer<Todo> documentMaterializer) : base(documentMaterializer)
        {
        }
    }
}