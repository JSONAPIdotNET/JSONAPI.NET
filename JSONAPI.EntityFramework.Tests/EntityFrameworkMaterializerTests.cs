using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.EntityFramework.Tests
{
    [TestClass]
    public class EntityFrameworkMaterializerTests
    {
        [TestMethod]
        public async Task Includes_one_to_many_navigation_properties_that_were_serialized()
        {
            using (var conn = TestHelpers.GetEffortConnection("Acceptance/Data"))
            {
                using (var context = new TestDbContext(conn, false))
                {
                    var metadataManager = new Mock<IMetadataManager>();
                    var materializer = new EntityFrameworkMaterializer(context, metadataManager.Object);

                    var ephemeral = new Post { Id = "201" };
                    var material = await materializer.MaterializeAsync(ephemeral);

                    material.Comments.Should().NotBeNull();
                    material.Comments.Count.Should().Be(3);
                }
            }
        }

        [TestMethod]
        public async Task Includes_many_to_many_navigation_properties_that_were_serialized()
        {
            using (var conn = TestHelpers.GetEffortConnection("Acceptance/Data"))
            {
                using (var context = new TestDbContext(conn, false))
                {
                    var metadataManager = new Mock<IMetadataManager>();
                    var materializer = new EntityFrameworkMaterializer(context, metadataManager.Object);

                    var ephemeral = new Post { Id = "201" };
                    var material = await materializer.MaterializeAsync(ephemeral);

                    material.Tags.Should().NotBeNull();
                    material.Tags.Count.Should().Be(2);
                }
            }
        }
    }
}
