using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class FetchingResourcesQueryResolverTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Data\CommentSearch.csv", @"Data")]
        [DeploymentItem(@"Data\PostSearch.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetAll()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "post-searchs");

                await AssertResponseContent(response, @"Fixtures\FetchingResourcesQueryResolver\GetAllResponse.json", HttpStatusCode.OK);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\CommentSearch.csv", @"Data")]
        [DeploymentItem(@"Data\PostSearch.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetWithFilter()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "post-searchs?filter[title]=Post efg");

                await AssertResponseContent(response, @"Fixtures\FetchingResourcesQueryResolver\GetWithFilterResponse.json", HttpStatusCode.OK);
            }
        }


        [TestMethod]
        [DeploymentItem(@"Data\CommentSearch.csv", @"Data")]
        [DeploymentItem(@"Data\PostSearch.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetWithSearchFilter() // this enables logic in Query resolver
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "post-searchs?searchterm=efg");

                await AssertResponseContent(response, @"Fixtures\FetchingResourcesQueryResolver\GetWithSearchFilterResponse.json", HttpStatusCode.OK);
            }
        }


        [TestMethod]
        [DeploymentItem(@"Data\CommentSearch.csv", @"Data")]
        [DeploymentItem(@"Data\PostSearch.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task GetWithSearchFilter_related_to_many() // this enables logic in Query resolver for related
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "post-searchs/201/comments?searchterm=efg");

                await AssertResponseContent(response, @"Fixtures\FetchingResourcesQueryResolver\GetWithSearchFilter_related_to_many_Response.json", HttpStatusCode.OK);
            }
        }

    }
}
