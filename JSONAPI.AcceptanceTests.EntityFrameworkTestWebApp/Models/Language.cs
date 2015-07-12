using System.ComponentModel.DataAnnotations;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class Language
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}