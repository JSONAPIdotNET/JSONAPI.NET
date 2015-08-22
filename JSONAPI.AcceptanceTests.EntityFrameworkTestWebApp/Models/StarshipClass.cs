using System.ComponentModel.DataAnnotations;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class StarshipClass
    {
        [Key]
        public string StarshipClassId { get; set; }

        public string Name { get; set; }
    }
}