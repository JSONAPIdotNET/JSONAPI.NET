using System.Collections.Generic;

namespace JSONAPI.Tests.Models
{
    class Author
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Post> Posts { get; set; }
    }
}
