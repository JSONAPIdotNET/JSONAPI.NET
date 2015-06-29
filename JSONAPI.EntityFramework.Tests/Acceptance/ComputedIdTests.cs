using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class ComputedIdTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Language.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\LanguageUserLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task Get_all_of_resource_with_computed_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "language-user-links");

                await AssertResponseContent(response, @"Acceptance\Fixtures\ComputedId\Responses\Get_all_of_resource_with_computed_id_Response.json", HttpStatusCode.OK);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Language.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\LanguageUserLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task Get_resource_with_computed_id_by_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "language-user-links/9001_402");

                await AssertResponseContent(response, @"Acceptance\Fixtures\ComputedId\Responses\Get_resource_with_computed_id_by_id_Response.json", HttpStatusCode.OK);
            }
        }
    }
}
