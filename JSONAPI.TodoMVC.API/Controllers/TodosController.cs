using JSONAPI.EntityFramework.Http;
using JSONAPI.TodoMVC.API.Models;

namespace JSONAPI.TodoMVC.API.Controllers
{
    public class TodosController : ApiController<Todo, TodoMvcContext>
    {
    }
}