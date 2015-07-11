using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class State
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        public virtual ICollection<City> Cities { get; set; }
    }
}