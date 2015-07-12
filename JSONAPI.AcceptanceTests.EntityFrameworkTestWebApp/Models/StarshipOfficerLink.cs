using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class StarshipOfficerLink
    {
        [Key, Column(Order = 0)]
        public string StarshipId { get; set; }

        [Key, Column(Order = 1)]
        public string OfficerId { get; set; }

        [ForeignKey("StarshipId")]
        public virtual Starship Starship { get; set; }

        [ForeignKey("OfficerId")]
        public virtual Officer Officer { get; set; }

        public string Position { get; set; }
    }
}