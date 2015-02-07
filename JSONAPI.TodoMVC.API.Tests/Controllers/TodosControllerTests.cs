using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.TodoMVC.API.Controllers;
using JSONAPI.TodoMVC.API.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.TodoMVC.API.Tests.Controllers
{
    [TestClass]
    public class TodosControllerTests
    {
        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv", "Data")]
        public void Get_returns_correct_data()
        {
            // Arrange
            using (var dbContext = new TodoMvcContext(TestHelpers.GetEffortConnection("Data"), true))
            {
                var controller = new TodosController(dbContext);

                // Act
                var result = controller.Get().ToArray();

                // Assert
                var expected = new[]
                {
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
                };
                result.ShouldBeEquivalentTo(expected);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv")]
        public async Task Get_by_id_returns_correct_data()
        {
            // Arrange
            using (var dbContext = new TodoMvcContext(TestHelpers.GetEffortConnection("Data"), true))
            {
                var controller = new TodosController(dbContext);

                // Act
                var result = await controller.Get("2");

                // Assert
                var expected = new[]
                {
                    new Todo
                    {
                        Id = "2",
                        Text = "Do the laundry",
                        IsCompleted = true
                    }
                };
                result.ShouldBeEquivalentTo(expected);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv")]
        public async Task Post_inserts_the_resource_and_returns_the_right_data()
        {
            // Arrange
            var newItem = new Todo
            {
                Id = "4",
                Text = "Eat breakfast",
                IsCompleted = false
            };

            using (var connection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var dbContext = new TodoMvcContext(connection, false))
                {
                    var controller = new TodosController(dbContext);
                    MetadataManager.Instance.SetMetaForProperty(newItem, typeof (Todo).GetProperty("Id"), true);
                    MetadataManager.Instance.SetMetaForProperty(newItem, typeof (Todo).GetProperty("Text"), true);
                    MetadataManager.Instance.SetMetaForProperty(newItem, typeof (Todo).GetProperty("IsCompleted"), true);

                    // Act
                    var result = await controller.Post(new List<Todo> {newItem});

                    // Assert
                    var expected = new[]
                    {
                        new Todo
                        {
                            Id = "4",
                            Text = "Eat breakfast",
                            IsCompleted = false
                        }
                    };
                    result.ShouldBeEquivalentTo(expected);
                }

                using (var dbContext = new TodoMvcContext(connection, false))
                {
                    var allTodos = dbContext.Todos.ToArray();
                    allTodos.Length.Should().Be(4);
                    var actualTodo = allTodos.First(t => t.Id == "4");
                    actualTodo.ShouldBeEquivalentTo(newItem);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv")]
        public async Task Put_updates_the_resource_and_returns_the_right_data()
        {
            // Arrange
            var updatedItem = new Todo
            {
                Id = "3",
                Text = null,
                IsCompleted = true
            };

            using (var connection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var dbContext = new TodoMvcContext(connection, false))
                {
                    var controller = new TodosController(dbContext);
                    MetadataManager.Instance.SetMetaForProperty(updatedItem, typeof(Todo).GetProperty("IsCompleted"), true);
                    // Purposely not setting text to test under-posting

                    // Act
                    var result = await controller.Put("3", new List<Todo> { updatedItem });

                    // Assert
                    var expected = new[]
                    {
                        new Todo
                        {
                            Id = "3",
                            Text = "Walk the dog",
                            IsCompleted = true
                        }
                    };
                    result.ShouldBeEquivalentTo(expected);
                }

                using (var dbContext = new TodoMvcContext(connection, false))
                {
                    var expectedItem = new Todo
                    {
                        Id = "3",
                        Text = "Walk the dog",
                        IsCompleted = true
                    };

                    var allTodos = dbContext.Todos.ToArray();
                    allTodos.Length.Should().Be(3);
                    var actualTodo = allTodos.First(t => t.Id == "3");
                    actualTodo.ShouldBeEquivalentTo(expectedItem);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv")]
        public async Task Delete_deletes_the_resource_and_returns_the_right_data()
        {
            // Arrange
            using (var connection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var dbContext = new TodoMvcContext(connection, false))
                {
                    var controller = new TodosController(dbContext);

                    // Act
                    await controller.Delete("2");
                }

                using (var dbContext = new TodoMvcContext(connection, false))
                {
                    var allTodos = dbContext.Todos.ToArray();
                    allTodos.Length.Should().Be(2);
                    var actualTodo = allTodos.FirstOrDefault(t => t.Id == "2");
                    actualTodo.Should().BeNull();
                }
            }
        }
    }
}
