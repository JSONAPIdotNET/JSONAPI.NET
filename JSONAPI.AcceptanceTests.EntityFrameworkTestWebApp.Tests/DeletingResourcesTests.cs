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
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
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
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(3);
                    var actualPosts = allPosts.FirstOrDefault(t => t.Id == "203");
                    actualPosts.Should().BeNull();
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\PostID.csv", @"Data")]
        public async Task DeleteID()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitDelete(effortConnection, "post-i-ds/203");

                var responseContent = await response.Content.ReadAsStringAsync();
                responseContent.Should().Be("");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.PostsID.ToArray();
                    allPosts.Length.Should().Be(3);
                    var actualPosts = allPosts.FirstOrDefault(t => t.ID == "203");
                    actualPosts.Should().BeNull();
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\PostLongId.csv", @"Data")]
        public async Task DeleteLongId()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitDelete(effortConnection, "post-long-ids/203");

                var responseContent = await response.Content.ReadAsStringAsync();
                responseContent.Should().Be("");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.PostsLongId.ToArray();
                    allPosts.Length.Should().Be(3);
                    var actualPosts = allPosts.FirstOrDefault(t => t.Id == 203);
                    actualPosts.Should().BeNull();
                }
            }
        }
    }
}
