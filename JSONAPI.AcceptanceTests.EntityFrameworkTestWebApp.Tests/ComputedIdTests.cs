using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class ComputedIdTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Data\Comment.csv", @"Data")]
        [DeploymentItem(@"Data\Language.csv", @"Data")]
        [DeploymentItem(@"Data\LanguageUserLink.csv", @"Data")]
        [DeploymentItem(@"Data\Post.csv", @"Data")]
        [DeploymentItem(@"Data\PostTagLink.csv", @"Data")]
        [DeploymentItem(@"Data\Tag.csv", @"Data")]
        [DeploymentItem(@"Data\User.csv", @"Data")]
        public async Task Get_all_of_resource_with_computed_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "language-user-links");

                await AssertResponseContent(response, @"Fixtures\ComputedId\Responses\Get_all_of_resource_with_computed_id_Response.json", HttpStatusCode.OK);
            }
        }

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
    }
}
