using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class PostsController : JsonApiController<Post>
    {
        public PostsController(IPayloadMaterializer<Post> payloadMaterializer) : base(payloadMaterializer)
        {
        }
    }
}