using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSONAPI.Core;
using JSONAPI.Attributes;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Models
{
    class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [IncludeInPayload(true)]
        public IList<Comment> Comments { get; set; }

        [IncludeInPayload(true)]
        public Author Author { get; set; }
    }
}
