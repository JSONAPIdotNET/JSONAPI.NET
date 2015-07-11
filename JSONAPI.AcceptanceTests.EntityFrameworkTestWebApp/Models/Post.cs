using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class Post
    {
        [Key]
        public string Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTimeOffset Created { get; set; }

        [JsonIgnore]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public virtual User Author { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public virtual ICollection<Tag> Tags { get; set; }
    }
}