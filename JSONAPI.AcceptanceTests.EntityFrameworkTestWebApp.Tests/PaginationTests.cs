using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class PaginationTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetPage2Post2()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "posts?page[number]=1&page[size]=2");

                await AssertResponseContent(response, @"Fixtures\Pagination\GetAllResponsePaged-2-2.json", HttpStatusCode.OK);
            }
        }

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
                var response = await SubmitGet(effortConnection, "users?filter[last-name]=Burns&page[number]=1&page[size]=1");

                await AssertResponseContent(response, @"Fixtures\Pagination\GetFilterPaged-2-1.json", HttpStatusCode.OK);
            }
        }


        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetWithFilterSortedAscTest()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "users?filter[last-name]=Burns&page[number]=0&page[size]=2&sort=first-name");

                await AssertResponseContent(response, @"Fixtures\Pagination\GetFilterPaged-1-2-sorted.json", HttpStatusCode.OK);
            }
        }


        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetWithFilterSortedDescTest()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "users?filter[last-name]=Burns&page[number]=0&page[size]=2&sort=-first-name");

                await AssertResponseContent(response, @"Fixtures\Pagination\GetFilterPaged-1-2-sorted-desc.json", HttpStatusCode.OK);
            }
        }

    }
}
