using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public class MappedTests : AcceptanceTestsBase
    {
        [TestMethod]
        [DeploymentItem(@"Data\Starship.csv", @"Data")]
        [DeploymentItem(@"Data\StarshipClass.csv", @"Data")]
        public async Task Get_all()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "starships");

                await AssertResponseContent(response, @"Fixtures\Mapped\Responses\Get_all.json", HttpStatusCode.OK);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Starship.csv", @"Data")]
        [DeploymentItem(@"Data\StarshipClass.csv", @"Data")]
        public async Task Get_by_id()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "starships/NCC-1701");

                await AssertResponseContent(response, @"Fixtures\Mapped\Responses\Get_by_id.json", HttpStatusCode.OK);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Starship.csv", @"Data")]
        [DeploymentItem(@"Data\StarshipClass.csv", @"Data")]
        public async Task Get_resource_by_id_that_doesnt_exist()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "starships/NCC-asdf");

                await AssertResponseContent(response, @"Fixtures\Mapped\Responses\Get_resource_by_id_that_doesnt_exist.json", HttpStatusCode.NotFound, true);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Starship.csv", @"Data")]
        [DeploymentItem(@"Data\StarshipClass.csv", @"Data")]
        [DeploymentItem(@"Data\Officer.csv", @"Data")]
        [DeploymentItem(@"Data\StarshipOfficerLink.csv", @"Data")]
        public async Task Get_related_to_many()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "starships/NCC-1701-D/officers");

                await AssertResponseContent(response, @"Fixtures\Mapped\Responses\Get_related_to_many_response.json", HttpStatusCode.OK);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\Starship.csv", @"Data")]
        [DeploymentItem(@"Data\StarshipClass.csv", @"Data")]
        [DeploymentItem(@"Data\Officer.csv", @"Data")]
        [DeploymentItem(@"Data\StarshipOfficerLink.csv", @"Data")]
        public async Task Get_related_to_one()
        {
            using (var effortConnection = GetEffortConnection())
            {
                var response = await SubmitGet(effortConnection, "starships/NCC-1701-D/ship-counselor");

                await AssertResponseContent(response, @"Fixtures\Mapped\Responses\Get_related_to_one_response.json", HttpStatusCode.OK);
            }
        }
    }
}
