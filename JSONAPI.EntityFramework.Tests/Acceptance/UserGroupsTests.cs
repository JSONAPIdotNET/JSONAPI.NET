using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class UserGroupsTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\UserGroup.csv", @"Acceptance\Data")]
        public async Task Get()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "user-groups");

                await AssertResponseContent(response, @"Acceptance\Fixtures\UserGroups\Responses\GetAllResponse.json", HttpStatusCode.OK);
            }
        }
    }
}
