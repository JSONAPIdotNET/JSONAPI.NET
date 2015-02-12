using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class SortingTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedAscending()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToSucceed(effortConnection, "users?sort=%2BfirstName", @"Acceptance\Fixtures\Users_GetSortedAscendingResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedDesending()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToSucceed(effortConnection, "users?sort=-firstName", @"Acceptance\Fixtures\Users_GetSortedDescendingResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedByMultipleAscending()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToSucceed(effortConnection, "users?sort=%2BlastName,%2BfirstName", @"Acceptance\Fixtures\Users_GetSortedByMultipleAscendingResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedByMultipleDescending()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToSucceed(effortConnection, "users?sort=-lastName,-firstName", @"Acceptance\Fixtures\Users_GetSortedByMultipleDescendingResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedByMixedDirection()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToSucceed(effortConnection, "users?sort=%2BlastName,-firstName", @"Acceptance\Fixtures\Users_GetSortedByMixedDirectionResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedByUnknownColumn()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToFail(effortConnection, "users?sort=%2Bfoobar", @"Acceptance\Fixtures\Users_GetSortedByUnknownColumnResponse.json");
            }
        }

        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedBySameColumnTwice()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToFail(effortConnection, "users?sort=%2BfirstName,%2BfirstName", @"Acceptance\Fixtures\Users_GetSortedBySameColumnTwiceResponse.json");
            }
        }
        
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task GetSortedByColumnMissingDirection()
        {
            using (var effortConnection = GetEffortConnection())
            {
                await ExpectGetToFail(effortConnection, "users?sort=firstName", @"Acceptance\Fixtures\Users_GetSortedByColumnMissingDirectionResponse.json");
            }
        }
    }
}
