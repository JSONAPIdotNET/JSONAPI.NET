using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using Newtonsoft.Json.Linq;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Controllers
{
    public class PresidentsController : ApiController
    {
        public class MyArrayPayload<T> : IPayload
        {
            private readonly T[] _array;

            public MyArrayPayload(T[] array)
            {
                _array = array;
            }

            public object PrimaryData { get { return _array; } }

            public JObject Metadata
            {
                get
                {
                    var obj = new JObject();
                    obj["count"] = _array.Length;
                    return obj;
                }
            }
        }

        // This endpoint exists to demonstrate returning IPayload
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

            var payload = new MyArrayPayload<User>(users);
            return Ok(payload);
        }
    }
}