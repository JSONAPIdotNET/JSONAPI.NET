using System.ComponentModel.DataAnnotations;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class Company
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}