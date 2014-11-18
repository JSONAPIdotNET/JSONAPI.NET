using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSONAPI.Core;
using JSONAPI.Attributes;

namespace JSONAPI.Tests.Models
{
    class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [IncludeInPayload(true)]
        public IList<Post> Posts { get; set; }
    }
}
