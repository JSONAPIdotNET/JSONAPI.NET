namespace JSONAPI.EntityFramework.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class Post
    {
        public Post()
        {
            this.Comments = new HashSet<Comment>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Title { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
