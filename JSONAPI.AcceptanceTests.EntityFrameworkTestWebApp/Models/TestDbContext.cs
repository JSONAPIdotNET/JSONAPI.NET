using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models
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

        public DbSet<Building> Buildings { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Language> Languages { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LanguageUserLink> LanguageUserLinks { get; set; }
        public DbSet<Starship> Starships { get; set; }
        public DbSet<StarshipClass> StarshipClasses { get; set; }
        public DbSet<Officer> Officers { get; set; }
        public DbSet<StarshipOfficerLink> StarshipOfficerLinks { get; set; }
    }
}