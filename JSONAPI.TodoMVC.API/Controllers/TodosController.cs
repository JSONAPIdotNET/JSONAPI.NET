using JSONAPI.Core;
using JSONAPI.EntityFramework;
using JSONAPI.EntityFramework.Http;
using JSONAPI.TodoMVC.API.Models;

namespace JSONAPI.TodoMVC.API.Controllers
{
    public class TodosController : ApiController<Todo, TodoMvcContext>
    {
        protected readonly TodoMvcContext DbContext;

        public TodosController(TodoMvcContext dbContext)
        {
            DbContext = dbContext;
        }

        protected override IMaterializer MaterializerFactory()
        {
            return new EntityFrameworkMaterializer(DbContext);
        }
    }
}