namespace JSONAPI.EntityFramework.Tests.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Author
    {
        public Author()
        {
            this.Posts = new HashSet<Post>();
        }
    
        public string Id { get; set; }
        public string Name { get; set; }
    
        public virtual ICollection<Post> Posts { get; set; }
    }
}
