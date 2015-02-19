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
                await ExpectGetToSucceed(effortConnection, "users?sort=%2BfirstName", @"Acceptance\Fixtures\Sorting\Responses\GetSortedAscendingResponse.json");
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
                await ExpectGetToSucceed(effortConnection, "users?sort=-firstName", @"Acceptance\Fixtures\Sorting\Responses\GetSortedDescendingResponse.json");
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
                await ExpectGetToSucceed(effortConnection, "users?sort=%2BlastName,%2BfirstName", @"Acceptance\Fixtures\Sorting\Responses\GetSortedByMultipleAscendingResponse.json");
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
                await ExpectGetToSucceed(effortConnection, "users?sort=-lastName,-firstName", @"Acceptance\Fixtures\Sorting\Responses\GetSortedByMultipleDescendingResponse.json");
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
                await ExpectGetToSucceed(effortConnection, "users?sort=%2BlastName,-firstName", @"Acceptance\Fixtures\Sorting\Responses\GetSortedByMixedDirectionResponse.json");
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
                await ExpectGetToFail(effortConnection, "users?sort=%2Bfoobar", @"Acceptance\Fixtures\Sorting\Responses\GetSortedByUnknownColumnResponse.json");
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
                await ExpectGetToFail(effortConnection, "users?sort=%2BfirstName,%2BfirstName", @"Acceptance\Fixtures\Sorting\Responses\GetSortedBySameColumnTwiceResponse.json");
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
                await ExpectGetToFail(effortConnection, "users?sort=firstName", @"Acceptance\Fixtures\Sorting\Responses\GetSortedByColumnMissingDirectionResponse.json");
            }
        }
    }
}
