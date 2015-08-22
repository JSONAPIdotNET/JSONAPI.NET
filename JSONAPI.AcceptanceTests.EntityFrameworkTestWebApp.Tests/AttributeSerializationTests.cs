using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class AttributeSerializationTests : AcceptanceTestsBase
    {
        [TestMethod]
        public async Task Attributes_of_various_types_serialize_correctly()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "samples");

                await AssertResponseContent(response, @"Fixtures\AttributeSerialization\Attributes_of_various_types_serialize_correctly.json", HttpStatusCode.OK);
            }
        }
    }
}
