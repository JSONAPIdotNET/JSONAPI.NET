using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.EntityFramework.Http;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.EntityFramework.Tests
{
    [TestClass]
    public class EntityFrameworkPayloadMaterializerTests
    {
        private static DbConnection GetEffortConnection()
        {
            return TestHelpers.GetEffortConnection(@"Acceptance\Data");
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Acceptance\Data\Comment.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Post.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\PostTagLink.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\Tag.csv", @"Acceptance\Data")]
        [DeploymentItem(@"Acceptance\Data\User.csv", @"Acceptance\Data")]
        public void GetRecords_returns_all_records_when_there_is_no_filtering()
        {
            //using (var effortConnection = GetEffortConnection())
            //{
            //    using (var dbContext = new TestDbContext(effortConnection, false))
            //    {
            //        // Arrange
            //        var mockResourceObjectReader = new Mock<IResourceObjectReader>(MockBehavior.Strict);
            //        var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/posts");
            //        var cts = new CancellationTokenSource();

            //        var mockPayload = new Mock<IResourceCollectionPayload>(MockBehavior.Strict);

            //        var mockQueryableBuilder = new Mock<IQueryableResourceCollectionPayloadBuilder>(MockBehavior.Strict);
            //        var mockSingleResourcePayloadBuilder = new Mock<ISingleResourcePayloadBuilder>(MockBehavior.Strict);

            //        // Act
            //        var materializer = new EntityFrameworkPayloadMaterializer(dbContext, mockResourceObjectReader.Object,
            //            mockQueryableBuilder.Object, mockSingleResourcePayloadBuilder.Object);
            //        var payload = materializer.GetRecords<Post>(request, cts.Token).Result;

            //        // Assert
            //        payload.Should().BeSameAs(mockPayload.Object);
            //    }
            //}
        }
    }
}
