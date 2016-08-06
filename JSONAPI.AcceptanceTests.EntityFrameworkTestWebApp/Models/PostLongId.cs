using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
{
    public class PostLongId
    {
        [Key]
        public long Id { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }

        public DateTimeOffset Created { get; set; }

    }
}