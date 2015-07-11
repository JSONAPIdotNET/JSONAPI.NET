using System.Web.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class CitiesController : ApiController
    {
        public IHttpActionResult Get(string id)
        {
            City city;
            if (id == "9000")
            {
                city =
                    new City
                    {
                        Id = "9000",
                        Name = "Seattle",
                        State = new State
                        {
                            Id = "4000",
                            Name = "Washington"
                        }
                    };
            }
            else if (id == "9001")
            {
                city =
                    new City
                    {
                        Id = "9001",
                        Name = "Tacoma"
                    };
            }
            else
            {
                return NotFound();
            }

            return Ok(city);
        }
    }
}