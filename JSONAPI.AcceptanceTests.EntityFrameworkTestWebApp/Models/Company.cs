using System.ComponentModel.DataAnnotations;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class Company
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}