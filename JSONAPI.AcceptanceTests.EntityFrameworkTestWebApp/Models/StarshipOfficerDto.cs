using Newtonsoft.Json;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    [JsonObject(Title = "starship-officer")]
    public class StarshipOfficerDto
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string Rank { get; set; }

        public string Position { get; set; }

        public virtual StarshipDto CurrentShip { get; set; }
    }
}