using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class Building
    {
        [Key]
        public string Id { get; set; }

        public string Address { get; set; }

        [JsonIgnore]
        public string OwnerCompanyId { get; set; }

        [ForeignKey("OwnerCompanyId")]
        public virtual Company Owner { get; set; }
    }
}