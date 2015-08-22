using System.Collections.Generic;
using Newtonsoft.Json;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    [JsonObject(Title = "starship")]
    public class StarshipDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string StarshipClass { get; set; }

        public virtual ICollection<StarshipOfficerDto> Officers { get; set; }

        public virtual StarshipOfficerDto ShipCounselor { get; set; }
    }
}