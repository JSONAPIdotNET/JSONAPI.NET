namespace JSONAPI.EntityFramework.Tests.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class PostID
    {
        public PostID()
        {
            this.Comments = new HashSet<Comment>();
        }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public string Title { get; set; }
    
        public virtual Author Author { get; set; }
        public virtual ICollection<Comment> Comments { get; set; }
    }
}
