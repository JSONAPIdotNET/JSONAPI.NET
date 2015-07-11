using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class ErrorsTests : AcceptanceTestsBase
    {
        [TestMethod]
        public async Task Controller_action_throws_exception()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "trees");

                await AssertResponseContent(response, @"Fixtures\Errors\Controller_action_throws_exception.json", HttpStatusCode.InternalServerError, true);
            }
        }

        [TestMethod]
        [Ignore]
        public async Task Controller_does_not_exist()
        {
            // TODO: Currently ignoring this test because it doesn't seem possible to intercept 404s before they make it to the formatter
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "foo");

                await AssertResponseContent(response, @"Fixtures\Errors\Controller_does_not_exist.json", HttpStatusCode.NotFound, true);
            }
        }
    }
}
