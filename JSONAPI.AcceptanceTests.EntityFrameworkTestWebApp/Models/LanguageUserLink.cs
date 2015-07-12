using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class LanguageUserLink
    {
        public string Id { get { return LanguageId + "_" + UserId; } }

        [JsonIgnore]
        [Key, Column(Order = 1)]
        public string LanguageId { get; set; }

        [JsonIgnore]
        [Key, Column(Order = 2)]
        public string UserId { get; set; }

        public string FluencyLevel { get; set; }

        [ForeignKey("LanguageId")]
        public virtual Language Language { get; set; }

        [ForeignKey("UserId")]
        public virtual User User { get; set; }
    }
}