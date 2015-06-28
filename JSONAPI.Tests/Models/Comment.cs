using JSONAPI.Attributes;

namespace JSONAPI.Tests.Models
{
    class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
        public Post Post { get; set; }
        [SerializeAsComplex]public string CustomData { get; set; }
    }
}
