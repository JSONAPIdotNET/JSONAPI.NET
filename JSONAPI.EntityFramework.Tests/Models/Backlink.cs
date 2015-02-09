using JSONAPI.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JSONAPI.EntityFramework.Tests.Models
{
    public class Backlink
    {
        [UseAsId]
        [System.ComponentModel.DataAnnotations.Key]
        public string Url { get; set; }

        public Post Post { get; set; }
        public string Snippet { get; set; }
    }
}
