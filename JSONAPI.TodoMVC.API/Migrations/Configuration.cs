using System.Data.Entity.Migrations;
using JSONAPI.TodoMVC.API.Models;

namespace JSONAPI.TodoMVC.API.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<TodoMvcContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(TodoMvcContext context)
        {
            context.Todos.AddOrUpdate(
                new Todo
                {
                    Id = "1",
                    Text = "Do the dishes",
                    IsCompleted = false
                },
                new Todo
                {
                    Id = "2",
                    Text = "Do the laundry",
                    IsCompleted = true
                },
                new Todo
                {
                    Id = "3",
                    Text = "Walk the dog",
                    IsCompleted = false
                }
            );
        }
    }
}
