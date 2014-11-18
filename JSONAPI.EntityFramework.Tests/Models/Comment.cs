namespace JSONAPI.EntityFramework.Tests.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Comment
    {
        public int Id { get; set; }
        public string Body { get; set; }
    
        public virtual Post Post { get; set; }
    }
}
