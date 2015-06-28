using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class TagsController : JsonApiController<Tag>
    {
        public TagsController(IPayloadMaterializer payloadMaterializer) : base(payloadMaterializer)
        {
        }
    }
}