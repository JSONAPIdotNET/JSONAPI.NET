using System.Linq;
using System.Web.Http;
using JSONAPI.Documents;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class PresidentsController : ApiController
    {
        // This endpoint exists to demonstrate returning IResourceCollectionDocument
        [Route("presidents")]
        public IHttpActionResult GetPresidents()
        {
            var users = new[]
            {
                new User
                {
                    Id = "6500",
                    FirstName = "George",
                    LastName = "Washington"
                },
                new User
                {
                    Id = "6501",
                    FirstName = "Abraham",
                    LastName = "Lincoln"
                }
            };
            
            var userResources = users.Select(u => (IResourceObject)new ResourceObject("users", u.Id)).ToArray();

            var document = new ResourceCollectionDocument(userResources, null, null);
            return Ok(document);
        }
    }
}