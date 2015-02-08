using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Post = JSONAPI.EntityFramework.Tests.TestWebApp.Models.Post;

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
        public async Task Get()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await TestGet(effortConnection, "posts", @"Acceptance\Fixtures\Posts_GetResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetWithFilter()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await TestGetWithFilter(effortConnection, "posts?title=Post 4", @"Acceptance\Fixtures\Posts_GetWithFilterResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetById()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await TestGetById(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts_GetByIdResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task Post()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await TestPost(effortConnection, "posts", @"Acceptance\Fixtures\Posts_PostRequest.json", @"Acceptance\Fixtures\Posts_PostResponse.json");

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
        public async Task Put()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await TestPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts_PutRequest.json", @"Acceptance\Fixtures\Posts_PutResponse.json");

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("New post title");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
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
        public async Task Delete()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await TestDelete(effortConnection, "posts/203");

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
