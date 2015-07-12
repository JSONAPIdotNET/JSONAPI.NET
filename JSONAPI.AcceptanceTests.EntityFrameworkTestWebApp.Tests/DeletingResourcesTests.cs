using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class DeletingResourcesTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\User.csv", @"Acceptance\Data")]
        public async Task Delete()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitDelete(effortConnection, "posts/203");

                var responseContent = await response.Content.ReadAsStringAsync();
                responseContent.Should().Be("");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allTodos = dbContext.Posts.ToArray();
                    allTodos.Length.Should().Be(3);
                    var actualTodo = allTodos.FirstOrDefault(t => t.Id == "203");
                    actualTodo.Should().BeNull();
                }
            }
        }
    }
}
