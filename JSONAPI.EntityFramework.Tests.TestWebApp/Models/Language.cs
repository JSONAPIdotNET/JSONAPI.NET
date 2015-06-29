using System.ComponentModel.DataAnnotations;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class Language
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}