using System.ComponentModel.DataAnnotations;
using JSONAPI.Attributes;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class City
    {
        [Key]
        public string Id { get; set; }

        public string Name { get; set; }

        [RelatedResourceLinkTemplate("/cities/{1}/state")]
        public virtual State State { get; set; }
    }
}