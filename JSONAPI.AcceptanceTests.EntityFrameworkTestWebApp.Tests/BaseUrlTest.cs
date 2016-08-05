using System;
using System.Data.SqlTypes;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Owin;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class BaseUrlTest : AcceptanceTestsBase
    {
        [TestInitialize]
        public void TestInit()
        {
            if (!BaseUri.AbsoluteUri.EndsWith("api/"))
            {
                BaseUri = new Uri(BaseUri.AbsoluteUri + "api/");
            }
        }
        [TestCleanup]
        public void TestCleanup()
        {
            if (BaseUri.AbsoluteUri.EndsWith("api/"))
            {
                BaseUri = new Uri(BaseUri.AbsoluteUri.Substring(0,BaseUri.AbsoluteUri.Length -4));
            }
        }

        // custom startup process for this test
        protected override void StartupConfiguration(Startup startup, IAppBuilder app)
        {

            var configuration = startup.BuildConfiguration();
            // here we add the custom BaseUrlServcie
            configuration.CustomBaseUrlService = new BaseUrlService("api");
            var configurator = startup.BuildAutofacConfigurator(app);
            var httpConfig = startup.BuildHttpConfiguration();
            startup.MergeAndSetupConfiguration(app, configurator, httpConfig, configuration);
        }

        // custom expected response method
        protected override string ExpectedResponse(string expectedResponseTextResourcePath)
        {
            var expected = base.ExpectedResponse(expectedResponseTextResourcePath);
            return Regex.Replace(expected, @"www\.example\.com\/", @"www.example.com/api/");
        }

        // copied some tests in here

        // copied from ComputedIdTests

        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Language.csv", @"Data")]
        [DeploymentItem(@"Data\LanguageUserLink.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task Get_resource_with_computed_id_by_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "language-user-links/9001_402");

                await AssertResponseContent(response, @"Fixtures\ComputedId\Responses\Get_resource_with_computed_id_by_id_Response.json", HttpStatusCode.OK);
            }
        }


        // copied from CreatingResourcesTests


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



        // copied from DeletingResourcesTests

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



        // copied from FetchingResourcesTests

        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetWithFilter()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "posts?filter[title]=Post 4");

                await AssertResponseContent(response, @"Fixtures\FetchingResources\GetWithFilterResponse.json", HttpStatusCode.OK);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetById()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "posts/202");

                await AssertResponseContent(response, @"Fixtures\FetchingResources\GetByIdResponse.json", HttpStatusCode.OK);
            }
        }

    }
}
