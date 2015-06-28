using System;
using System.Web.Http;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class TreesController : ApiController
    {
        public IHttpActionResult Get()
        {
            throw new Exception("Something bad happened!");
        }
    }
}