using System;
using System.Data.Entity;
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
        public async Task GetAll()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "posts");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\GetAllResponse.json", HttpStatusCode.OK);
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
                var response = await SubmitGet(effortConnection, "posts?title=Post 4");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\GetWithFilterResponse.json", HttpStatusCode.OK);
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
                var response = await SubmitGet(effortConnection, "posts/202");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\GetByIdResponse.json", HttpStatusCode.OK);
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
                var response = await SubmitPost(effortConnection, "posts", @"Acceptance\Fixtures\Posts\Requests\PostRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PostResponse.json", HttpStatusCode.OK);

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
        public async Task PutWithAttributeUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithAttributeUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithAttributeUpdateResponse.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.Include(p => p.Tags).ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("New post title");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithToManyUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithToManyUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithToManyUpdateResponse.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.Include(p => p.Tags).ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("301");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithToManyHomogeneousDataUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithToManyHomogeneousDataUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithToManyHomogeneousDataUpdateResponse.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.Include(p => p.Tags).ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("301", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithToManyEmptyDataUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithToManyEmptyDataUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithToManyEmptyDataUpdateResponse.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.Include(p => p.Tags).ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Should().BeEmpty();
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithToOneUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithToOneUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithToOneUpdateResponse.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("403");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithNullToOneUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithNullToOneUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithNullToOneUpdateResponse.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().BeNull();
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithMissingToOneId()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithMissingToOneIdRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithMissingToOneIdResponse.json", HttpStatusCode.BadRequest);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithMissingToOneType()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithMissingToOneTypeRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithMissingToOneTypeResponse.json", HttpStatusCode.BadRequest);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithMissingToManyIds()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithMissingToManyIdsRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithMissingToManyIdsResponse.json", HttpStatusCode.BadRequest);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithMissingToManyType()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithMissingToManyTypeRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithMissingToManyTypeResponse.json", HttpStatusCode.BadRequest);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithArrayRelationshipValue()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithArrayRelationshipValueRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithArrayRelationshipValueResponse.json", HttpStatusCode.BadRequest);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
                }
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PutWithStringRelationshipValue()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPut(effortConnection, "posts/202", @"Acceptance\Fixtures\Posts\Requests\PutWithStringRelationshipValueRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Posts\Responses\PutWithStringRelationshipValueResponse.json", HttpStatusCode.BadRequest);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.AuthorId.Should().Be("401");
                    actualPost.Tags.Select(t => t.Id).Should().BeEquivalentTo("302", "303");
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
                var response = await SubmitDelete(effortConnection, "posts/203");

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
