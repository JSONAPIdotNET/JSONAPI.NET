using System;
using System.Web.Http;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Controllers
{
    public class TreesController : ApiController
    {
        public IHttpActionResult Get()
        {
            throw new Exception("Something bad happened!");
        }
    }
}