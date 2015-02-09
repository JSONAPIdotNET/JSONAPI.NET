using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace JSONAPI.TodoMVC.API.Models
{
    public class TodoMvcContext : DbContext
    {
        public TodoMvcContext()
        {
            
        }

        public TodoMvcContext(DbConnection connection, bool contextOwnsConnection)
            : base(connection, contextOwnsConnection)
        {
            
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Todo> Todos { get; set; }
    }
}