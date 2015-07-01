using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class UserGroupsController : JsonApiController<UserGroup>
    {
        public UserGroupsController(IDocumentMaterializer<UserGroup> documentMaterializer) : base(documentMaterializer)
        {
        }
    }
}