using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class CreatingResourcesTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task Post_with_client_provided_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPost(effortConnection, "posts", @"Fixtures\CreatingResources\Requests\Post_with_client_provided_id_Request.json");

                await AssertResponseContent(response, @"Fixtures\CreatingResources\Responses\Post_with_client_provided_id_Response.json", HttpStatusCode.OK);

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
        [DeploymentItem(@"Data\PostID.csv", @"Data")]
        public async Task PostID_with_client_provided_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPost(effortConnection, "post-i-ds", @"Fixtures\CreatingResources\Requests\PostID_with_client_provided_id_Request.json");

                await AssertResponseContent(response, @"Fixtures\CreatingResources\Responses\PostID_with_client_provided_id_Response.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.PostsID.ToArray();
                    allPosts.Length.Should().Be(5);
                    var actualPost = allPosts.First(t => t.ID == "205");
                    actualPost.ID.Should().Be("205");
                    actualPost.Title.Should().Be("Added post");
                    actualPost.Content.Should().Be("Added post content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 03, 11, 04, 31, 0, new TimeSpan(0)));
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\PostLongId.csv", @"Data")]
        public async Task PostLongId_with_client_provided_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPost(effortConnection, "post-long-ids", @"Fixtures\CreatingResources\Requests\PostLongId_with_client_provided_id_Request.json");

                await AssertResponseContent(response, @"Fixtures\CreatingResources\Responses\PostLongId_with_client_provided_id_Response.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.PostsLongId.ToArray();
                    allPosts.Length.Should().Be(5);
                    var actualPost = allPosts.First(t => t.Id == 205);
                    actualPost.Id.Should().Be(205);
                    actualPost.Title.Should().Be("Added post");
                    actualPost.Content.Should().Be("Added post content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 03, 11, 04, 31, 0, new TimeSpan(0)));
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task Post_with_empty_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPost(effortConnection, "posts", @"Fixtures\CreatingResources\Requests\Post_with_empty_id_Request.json");

                await AssertResponseContent(response, @"Fixtures\CreatingResources\Responses\Post_with_empty_id_Response.json", HttpStatusCode.OK);

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


        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task Post_with_empty_id_and_include()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPost(effortConnection, "posts?include=author", @"Fixtures\CreatingResources\Requests\Post_with_empty_id_Request.json");

                await AssertResponseContent(response, @"Fixtures\CreatingResources\Responses\Post_with_empty_id_and_include_author_Response.json", HttpStatusCode.OK);

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
