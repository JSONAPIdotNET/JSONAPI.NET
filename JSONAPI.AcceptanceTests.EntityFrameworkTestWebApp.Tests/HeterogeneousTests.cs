using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class HeterogeneousTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Data\User.csv", @"Acceptance\Data")]
        public async Task Get()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "search?s=1");

                await AssertResponseContent(response, @"Fixtures\Heterogeneous\Responses\GetSearchResultsResponse.json", HttpStatusCode.OK);
            }
        }
    }
}
