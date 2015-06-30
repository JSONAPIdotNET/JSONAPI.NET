using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class PostsTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task Post_with_client_provided_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPost(effortConnection, "posts", @"Acceptance\Fixtures\CreatingResources\Requests\Post_with_client_provided_id_Request.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\CreatingResources\Responses\Post_with_client_provided_id_Response.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(5);
                    var actualPost = allPosts.First(t => t.Id == "205");
                    actualPost.Id.Should().Be("205");
                    actualPost.Title.Should().Be("Added post");
                    actualPost.Content.Should().Be("Added post content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 03, 11, 04, 31, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task Post_with_empty_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPost(effortConnection, "posts", @"Acceptance\Fixtures\CreatingResources\Requests\Post_with_empty_id_Request.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\CreatingResources\Responses\Post_with_empty_id_Response.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(5);
                    var actualPost = allPosts.First(t => t.Id == "230");
                    actualPost.Id.Should().Be("230");
                    actualPost.Title.Should().Be("New post");
                    actualPost.Content.Should().Be("The server generated my ID");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 04, 13, 12, 09, 0, new TimeSpan(0, 3, 0, 0)));
                    actualPost.AuthorId.Should().Be("401");
                }
            }
        }
    }
}
