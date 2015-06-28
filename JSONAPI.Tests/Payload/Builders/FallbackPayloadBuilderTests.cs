using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Payload.Builders
{
    [TestClass]
    public class FallbackPayloadBuilderTests
    {
        private const string GuidRegex = @"\b[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}\b";

        class Fruit
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        [TestMethod]
        public async Task Creates_single_resource_payload_for_registered_non_collection_types()
        {
            // Arrange
            var objectContent = new Fruit { Id = "984", Name = "Kiwi" };

            var mockPayload = new Mock<ISingleResourcePayload>(MockBehavior.Strict);

            var singleResourcePayloadBuilder = new Mock<ISingleResourcePayloadBuilder>(MockBehavior.Strict);
            singleResourcePayloadBuilder.Setup(b => b.BuildPayload(objectContent, It.IsAny<string>(), null)).Returns(mockPayload.Object);

            var mockQueryablePayloadBuilder = new Mock<IQueryableResourceCollectionPayloadBuilder>(MockBehavior.Strict);
            var mockResourceCollectionPayloadBuilder = new Mock<IResourceCollectionPayloadBuilder>(MockBehavior.Strict);

            var cancellationTokenSource = new CancellationTokenSource();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/fruits");
            var mockRequestContext = new Mock<HttpRequestContext>();
            mockRequestContext.Setup(c => c.VirtualPathRoot).Returns("https://www.example.com/fruits");
            request.SetRequestContext(mockRequestContext.Object);

            // Act
            var fallbackPayloadBuilder = new FallbackPayloadBuilder(singleResourcePayloadBuilder.Object,
                mockQueryablePayloadBuilder.Object, mockResourceCollectionPayloadBuilder.Object);
            var resultPayload = await fallbackPayloadBuilder.BuildPayload(objectContent, request, cancellationTokenSource.Token);

            // Assert
            resultPayload.Should().BeSameAs(mockPayload.Object);
        }

        [TestMethod]
        public async Task Creates_resource_collection_payload_for_queryables()
        {
            // Arrange
            var items = new[]
            {
                new Fruit {Id = "43", Name = "Strawberry"},
                new Fruit {Id = "43", Name = "Grape"}
            }.AsQueryable();

            var mockPayload = new Mock<IResourceCollectionPayload>(MockBehavior.Strict);

            var singleResourcePayloadBuilder = new Mock<ISingleResourcePayloadBuilder>(MockBehavior.Strict);

            var request = new HttpRequestMessage();
            
            var cancellationTokenSource = new CancellationTokenSource();

            var mockQueryablePayloadBuilder = new Mock<IQueryableResourceCollectionPayloadBuilder>(MockBehavior.Strict);
            mockQueryablePayloadBuilder
                .Setup(b => b.BuildPayload(items, request, cancellationTokenSource.Token))
                .Returns(() => Task.FromResult(mockPayload.Object));

            var mockResourceCollectionPayloadBuilder = new Mock<IResourceCollectionPayloadBuilder>(MockBehavior.Strict);

            // Act
            var fallbackPayloadBuilder = new FallbackPayloadBuilder(singleResourcePayloadBuilder.Object,
                mockQueryablePayloadBuilder.Object, mockResourceCollectionPayloadBuilder.Object);
            var resultPayload = await fallbackPayloadBuilder.BuildPayload(items, request, cancellationTokenSource.Token);

            // Assert
            resultPayload.Should().BeSameAs(mockPayload.Object);
        }

        [TestMethod]
        public async Task Creates_resource_collection_payload_for_non_queryable_enumerables()
        {
            // Arrange
            var items = new[]
            {
                new Fruit {Id = "43", Name = "Strawberry"},
                new Fruit {Id = "43", Name = "Grape"}
            };

            var mockPayload = new Mock<IResourceCollectionPayload>(MockBehavior.Strict);

            var singleResourcePayloadBuilder = new Mock<ISingleResourcePayloadBuilder>(MockBehavior.Strict);

            var cancellationTokenSource = new CancellationTokenSource();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/fruits");

            var mockQueryablePayloadBuilder = new Mock<IQueryableResourceCollectionPayloadBuilder>(MockBehavior.Strict);
            var mockResourceCollectionPayloadBuilder = new Mock<IResourceCollectionPayloadBuilder>(MockBehavior.Strict);
            mockResourceCollectionPayloadBuilder
                .Setup(b => b.BuildPayload(items, "https://www.example.com/", It.IsAny<string[]>()))
                .Returns(() => (mockPayload.Object));

            // Act
            var fallbackPayloadBuilder = new FallbackPayloadBuilder(singleResourcePayloadBuilder.Object,
                mockQueryablePayloadBuilder.Object, mockResourceCollectionPayloadBuilder.Object);
            var resultPayload = await fallbackPayloadBuilder.BuildPayload(items, request, cancellationTokenSource.Token);

            // Assert
            resultPayload.Should().BeSameAs(mockPayload.Object);
        }
    }
}
