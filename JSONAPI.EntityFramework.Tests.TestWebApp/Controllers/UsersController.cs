using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class UsersController : JsonApiController<User>
    {
        public UsersController(IPayloadMaterializer<User> payloadMaterializer) : base(payloadMaterializer)
        {
        }
    }
}