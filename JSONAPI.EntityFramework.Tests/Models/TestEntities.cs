namespace JSONAPI.EntityFramework.Tests.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TestEntities : DbContext
    {
        public TestEntities()
            : base("name=TestEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Author> Authors { get; set; }
        public virtual DbSet<Post> Posts { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
    }
}
