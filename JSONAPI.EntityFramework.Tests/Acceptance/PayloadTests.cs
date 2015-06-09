using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public class PayloadTests : AcceptanceTestsBase
    {
        [TestMethod]
        public async Task Get_returns_IPayload()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "presidents");

                await AssertResponseContent(response, @"Acceptance\Fixtures\Payload\Responses\GetReturnsIPayloadResponse.json", HttpStatusCode.OK);
            }
        }
    }
}
