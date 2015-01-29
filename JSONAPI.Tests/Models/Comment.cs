using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JSONAPI.Attributes;
using JSONAPI.Core;

namespace JSONAPI.Tests.Models
{
    class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public Post Post { get; set; }
        [SerializeStringAsRawJson]public string CustomData { get; set; }
    }
}
