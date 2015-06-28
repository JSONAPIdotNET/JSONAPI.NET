using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class UserGroupsController : JsonApiController<UserGroup>
    {
        public UserGroupsController(IPayloadMaterializer payloadMaterializer) : base(payloadMaterializer)
        {
        }
    }
}