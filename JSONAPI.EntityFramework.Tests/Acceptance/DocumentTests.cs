using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class DocumentTests : AcceptanceTestsBase
    {
        [TestMethod]
        public async Task Get_returns_IResourceCollectionDocument()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "presidents");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Document\Responses\Get_returns_IResourceCollectionDocument.json", HttpStatusCode.OK);
            }
        }
    }
}
