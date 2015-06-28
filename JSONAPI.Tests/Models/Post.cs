using System.Collections.Generic;
using JSONAPI.Attributes;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Models
{
    class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }

        [JsonIgnore]
        public string AuthorId { get; set; }

        [RelationshipLinkTemplate("/posts/{1}/relationships/comments")]
        [RelatedResourceLinkTemplate("/posts/{1}/comments")]
        public IList<Comment> Comments { get; set; }
        public Author Author { get; set; }
    }
}
