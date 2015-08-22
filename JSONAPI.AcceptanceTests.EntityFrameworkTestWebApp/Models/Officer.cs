using System.ComponentModel.DataAnnotations;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class Officer
    {
        [Key]
        public string OfficerId { get; set; }

        public string Name { get; set; }

        public string Rank { get; set; }
    }
}