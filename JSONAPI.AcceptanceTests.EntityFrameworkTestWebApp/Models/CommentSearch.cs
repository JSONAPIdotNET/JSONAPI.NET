﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class CommentSearch
    {
        [Key]
        public string Id { get; set; }

        public string Text { get; set; }

        public DateTimeOffset Created { get; set; }

        [Required]
        [JsonIgnore]
        public string PostId { get; set; }

        [Required]
        [JsonIgnore]
        public string AuthorId { get; set; }

        [ForeignKey("PostId")]
        public virtual PostSearch Post { get; set; }
    }
}