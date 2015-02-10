using System.Threading.Tasks;
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
                await TestGet(effortConnection, "user-groups", @"Acceptance\Fixtures\UserGroups_GetResponse.json");
            }
        }
    }
}
