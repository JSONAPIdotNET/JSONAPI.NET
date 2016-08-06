using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class Child
    {
        public int Id { get; set; }

        public string ChildDescription { get; set; }

        [Required]
        [JsonIgnore]
        public int MasterId { get; set; }

        [ForeignKey("MasterId")]
        public Master Master { get; set; }
    }
}