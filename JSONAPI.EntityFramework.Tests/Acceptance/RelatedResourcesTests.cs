using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class RelatedResourcesTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public async Task Get_related_to_many()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "posts/201/comments");

                await AssertResponseContent(response, @"Acceptance\Fixtures\RelatedResources\Responses\Get_related_to_many_response.json", HttpStatusCode.OK);
            }
        }
    }
}