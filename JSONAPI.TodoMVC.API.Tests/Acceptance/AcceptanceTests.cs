using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Json;
using JSONAPI.TodoMVC.API.Models;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.TodoMVC.API.Tests.Acceptance
{
    [TestClass]
    public class AcceptanceTests
    {
        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv", "Data")]
        public async Task Get()
        {
            using (var effortConnection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var server = TestServer.Create(app =>
                {
                    var startup = new Startup(context => new TodoMvcContext(effortConnection, false));
                    startup.Configuration(app);
                }))
                {
                    var response = await server.CreateRequest("http://localhost/todos").GetAsync();
                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var expected =
                        JsonHelpers.MinifyJson(
                            TestHelpers.ReadEmbeddedFile(@"Acceptance\Fixtures\GetResponse.json"));
                    responseContent.Should().Be(expected);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv", "Data")]
        public async Task GetById()
        {
            using (var effortConnection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var server = TestServer.Create(app =>
                {
                    var startup = new Startup(context => new TodoMvcContext(effortConnection, false));
                    startup.Configuration(app);
                }))
                {
                    var response = await server.CreateRequest("http://localhost/todos/2").GetAsync();
                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var expected =
                        JsonHelpers.MinifyJson(
                            TestHelpers.ReadEmbeddedFile(@"Acceptance\Fixtures\GetByIdResponse.json"));
                    responseContent.Should().Be(expected);
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv", "Data")]
        public async Task Post()
        {
            using (var effortConnection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var server = TestServer.Create(app =>
                {
                    var startup = new Startup(context => new TodoMvcContext(effortConnection, false));
                    startup.Configuration(app);
                }))
                {
                    var requestContent =
                        JsonHelpers.MinifyJson(
                            TestHelpers.ReadEmbeddedFile(@"Acceptance\Fixtures\PostRequest.json"));
                    var response = await server
                        .CreateRequest("http://localhost/todos")
                        .And(request =>
                        {
                            request.Content = new StringContent(requestContent, Encoding.UTF8, "application/vnd.api+json");
                        })
                        .PostAsync();
                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var expected =
                        JsonHelpers.MinifyJson(
                            TestHelpers.ReadEmbeddedFile(@"Acceptance\Fixtures\PostResponse.json"));
                    responseContent.Should().Be(expected);
                }

                using (var dbContext = new TodoMvcContext(effortConnection, false))
                {
                    var allTodos = dbContext.Todos.ToArray();
                    allTodos.Length.Should().Be(4);
                    var actualTodo = allTodos.First(t => t.Id == "4");
                    actualTodo.ShouldBeEquivalentTo(new Todo
                    {
                        Id = "4",
                        Text = "Go to the gym",
                        IsCompleted = false
                    });
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv", "Data")]
        public async Task Put()
        {
            using (var effortConnection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var server = TestServer.Create(app =>
                {
                    var startup = new Startup(context => new TodoMvcContext(effortConnection, false));
                    startup.Configuration(app);
                }))
                {
                    var requestContent =
                        JsonHelpers.MinifyJson(
                            TestHelpers.ReadEmbeddedFile(@"Acceptance\Fixtures\PutRequest.json"));
                    var response = await server
                        .CreateRequest("http://localhost/todos/3")
                        .And(request =>
                        {
                            request.Content = new StringContent(requestContent, Encoding.UTF8,
                                "application/vnd.api+json");
                        }).SendAsync("PUT");
                    response.StatusCode.Should().Be(HttpStatusCode.OK);
                    var responseContent = await response.Content.ReadAsStringAsync();

                    var expected =
                        JsonHelpers.MinifyJson(
                            TestHelpers.ReadEmbeddedFile(@"Acceptance\Fixtures\PutResponse.json"));
                    responseContent.Should().Be(expected);
                }

                using (var dbContext = new TodoMvcContext(effortConnection, false))
                {
                    var allTodos = dbContext.Todos.ToArray();
                    allTodos.Length.Should().Be(3);
                    var actualTodo = allTodos.First(t => t.Id == "3");
                    actualTodo.ShouldBeEquivalentTo(new Todo
                    {
                        Id = "3",
                        Text = "Walk the dog",
                        IsCompleted = true
                    });
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Todo.csv", "Data")]
        public async Task Delete()
        {
            using (var effortConnection = TestHelpers.GetEffortConnection("Data"))
            {
                using (var server = TestServer.Create(app =>
                {
                    var startup = new Startup(context => new TodoMvcContext(effortConnection, false));
                    startup.Configuration(app);
                }))
                {
                    var response = await server
                        .CreateRequest("http://localhost/todos/1")
                        .SendAsync("DELETE");
                    response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                }

                using (var dbContext = new TodoMvcContext(effortConnection, false))
                {
                    var allTodos = dbContext.Todos.ToArray();
                    allTodos.Length.Should().Be(2);
                    var actualTodo = allTodos.FirstOrDefault(t => t.Id == "1");
                    actualTodo.Should().BeNull();
                }
            }
        }
    }
}
