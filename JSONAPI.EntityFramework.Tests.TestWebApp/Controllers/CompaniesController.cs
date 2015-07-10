using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class CompaniesController : JsonApiController<Company>
    {
        public CompaniesController(IDocumentMaterializer<Company> documentMaterializer)
            : base(documentMaterializer)
        {
        }
    }
}