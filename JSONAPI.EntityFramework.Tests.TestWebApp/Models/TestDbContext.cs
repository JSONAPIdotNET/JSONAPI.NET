using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace JSONAPI.EntityFramework.Tests.TestWebApp.Models
{
    public class TestDbContext : DbContext
    {
        public TestDbContext()
        {
            
        }

        public TestDbContext(string connectionString) : base(connectionString)
        {
            
        }

        public TestDbContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            modelBuilder
                .Entity<Post>()
                .HasMany(p => p.Tags)
                .WithMany(t => t.Posts)
                .Map(c => c.MapLeftKey("PostId").MapRightKey("TagId").ToTable("PostTagLink"));
        }

        public DbSet<Comment> Comments { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
    }
}