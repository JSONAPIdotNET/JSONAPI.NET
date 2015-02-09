using System.ComponentModel.DataAnnotations;

namespace JSONAPI.TodoMVC.API.Models
{
    public class Todo
    {
        public string Id { get; set; }

        [Required]
        public string Text { get; set; }

        public bool IsCompleted { get; set; }
    }
}