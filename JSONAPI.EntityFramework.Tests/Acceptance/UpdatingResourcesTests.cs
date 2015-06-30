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
    public class UpdatingResourcesTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task PatchWithAttributeUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithAttributeUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithAttributeUpdateResponse.json", HttpStatusCode.OK);

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
        public async Task PatchWithToManyUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToManyUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToManyUpdateResponse.json", HttpStatusCode.OK);

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
        public async Task PatchWithToManyHomogeneousDataUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToManyHomogeneousDataUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToManyHomogeneousDataUpdateResponse.json", HttpStatusCode.OK);

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
        public async Task PatchWithToManyEmptyLinkageUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToManyEmptyLinkageUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToManyEmptyLinkageUpdateResponse.json", HttpStatusCode.OK);

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
        public async Task PatchWithToOneUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToOneUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToOneUpdateResponse.json", HttpStatusCode.OK);

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
        public async Task PatchWithNullToOneUpdate()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithNullToOneUpdateRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithNullToOneUpdateResponse.json", HttpStatusCode.OK);

                using (var dbContext = new TestDbContext(effortConnection, false))
                {
                    var allPosts = dbContext.Posts.Include(p => p.Author).ToArray();
                    allPosts.Length.Should().Be(4);
                    var actualPost = allPosts.First(t => t.Id == "202");
                    actualPost.Id.Should().Be("202");
                    actualPost.Title.Should().Be("Post 2");
                    actualPost.Content.Should().Be("Post 2 content");
                    actualPost.Created.Should().Be(new DateTimeOffset(2015, 02, 05, 08, 10, 0, new TimeSpan(0)));
                    actualPost.Author.Should().BeNull();
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
        public async Task PatchWithMissingToOneLinkage()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithMissingToOneLinkageRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithMissingToOneLinkageResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithToOneLinkageObjectMissingId()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToOneLinkageObjectMissingIdRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToOneLinkageObjectMissingIdResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithToOneLinkageObjectMissingType()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToOneLinkageObjectMissingTypeRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToOneLinkageObjectMissingTypeResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithArrayForToOneLinkage()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithArrayForToOneLinkageRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithArrayForToOneLinkageResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithStringForToOneLinkage()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithStringForToOneLinkageRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithStringForToOneLinkageResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithMissingToManyLinkage()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithMissingToManyLinkageRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithMissingToManyLinkageResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithToManyLinkageObjectMissingId()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToManyLinkageObjectMissingIdRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToManyLinkageObjectMissingIdResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithToManyLinkageObjectMissingType()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithToManyLinkageObjectMissingTypeRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithToManyLinkageObjectMissingTypeResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithObjectForToManyLinkage()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithObjectForToManyLinkageRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithObjectForToManyLinkageResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithStringForToManyLinkage()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithStringForToManyLinkageRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithStringForToManyLinkageResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithNullForToManyLinkage()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithNullForToManyLinkageRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithNullForToManyLinkageResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithArrayRelationshipValue()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithArrayRelationshipValueRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithArrayRelationshipValueResponse.json", HttpStatusCode.BadRequest, true);

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
        public async Task PatchWithStringRelationshipValue()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitPatch(effortConnection, "posts/202", @"Acceptance\Fixtures\UpdatingResources\Requests\PatchWithStringRelationshipValueRequest.json");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UpdatingResources\Responses\PatchWithStringRelationshipValueResponse.json", HttpStatusCode.BadRequest, true);

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
    }
}
