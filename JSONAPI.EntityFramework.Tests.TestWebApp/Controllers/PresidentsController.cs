using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Payload;
using Newtonsoft.Json.Linq;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class PresidentsController : ApiController
    {
        // This endpoint exists to demonstrate returning IResourceCollectionPayload
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

            var payload = new ResourceCollectionPayload(userResources, null, null);
            return Ok(payload);
        }
    }
}