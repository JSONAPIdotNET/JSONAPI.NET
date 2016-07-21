using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class Master
    {
        public int Id { get; set; }

        public string Description { get; set; }

        public virtual ICollection<Child> Children { get; set; }
    }
}