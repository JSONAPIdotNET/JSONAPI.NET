using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class Starship
    {
        [Key]
        public string StarshipId { get; set; }

        public string Name { get; set; }

        public string StarshipClassId { get; set; }

        [ForeignKey("StarshipClassId")]
        public virtual StarshipClass StarshipClass { get; set; }

        public virtual ICollection<StarshipOfficerLink> OfficerLinks { get; set; }
    }
}