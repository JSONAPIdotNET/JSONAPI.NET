using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using JSONAPI.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Documents.Builders
{
    [TestClass]
    public class FallbackDocumentBuilderTests
    {
        private const string GuidRegex = @"\b[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}\b";

        class Fruit
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        [TestMethod]
        public async Task Creates_single_resource_document_for_registered_non_collection_types()
        {
            // Arrange
            var objectContent = new Fruit { Id = "984", Name = "Kiwi" };

            var mockDocument = new Mock<ISingleResourceDocument>(MockBehavior.Strict);

            var singleResourceDocumentBuilder = new Mock<ISingleResourceDocumentBuilder>(MockBehavior.Strict);
            singleResourceDocumentBuilder.Setup(b => b.BuildDocument(objectContent, It.IsAny<string>(), null, null)).Returns(mockDocument.Object);

            var mockQueryableDocumentBuilder = new Mock<IQueryableResourceCollectionDocumentBuilder>(MockBehavior.Strict);
            var mockResourceCollectionDocumentBuilder = new Mock<IResourceCollectionDocumentBuilder>(MockBehavior.Strict);

            var cancellationTokenSource = new CancellationTokenSource();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/fruits");
            var mockBaseUrlService = new Mock<IBaseUrlService>(MockBehavior.Strict);
            mockBaseUrlService.Setup(s => s.GetBaseUrl(request)).Returns("https://www.example.com");

            // Act
            var fallbackDocumentBuilder = new FallbackDocumentBuilder(singleResourceDocumentBuilder.Object,
                mockQueryableDocumentBuilder.Object, mockResourceCollectionDocumentBuilder.Object, mockBaseUrlService.Object);
            var resultDocument = await fallbackDocumentBuilder.BuildDocument(objectContent, request, cancellationTokenSource.Token);

            // Assert
            resultDocument.Should().BeSameAs(mockDocument.Object);
        }

        [TestMethod]
        public async Task Creates_resource_collection_document_for_queryables()
        {
            // Arrange
            var items = new[]
            {
                new Fruit {Id = "43", Name = "Strawberry"},
                new Fruit {Id = "43", Name = "Grape"}
            }.AsQueryable();

            var mockDocument = new Mock<IResourceCollectionDocument>(MockBehavior.Strict);

            var singleResourceDocumentBuilder = new Mock<ISingleResourceDocumentBuilder>(MockBehavior.Strict);

            var request = new HttpRequestMessage();

            var mockBaseUrlService = new Mock<IBaseUrlService>(MockBehavior.Strict);
            mockBaseUrlService.Setup(s => s.GetBaseUrl(request)).Returns("https://www.example.com/");
            
            var cancellationTokenSource = new CancellationTokenSource();

            var mockQueryableDocumentBuilder = new Mock<IQueryableResourceCollectionDocumentBuilder>(MockBehavior.Strict);
            mockQueryableDocumentBuilder
                .Setup(b => b.BuildDocument(items, request, cancellationTokenSource.Token, null))
                .Returns(Task.FromResult(mockDocument.Object));

            var mockResourceCollectionDocumentBuilder = new Mock<IResourceCollectionDocumentBuilder>(MockBehavior.Strict);

            // Act
            var fallbackDocumentBuilder = new FallbackDocumentBuilder(singleResourceDocumentBuilder.Object,
                mockQueryableDocumentBuilder.Object, mockResourceCollectionDocumentBuilder.Object, mockBaseUrlService.Object);
            var resultDocument = await fallbackDocumentBuilder.BuildDocument(items, request, cancellationTokenSource.Token);

            // Assert
            resultDocument.Should().BeSameAs(mockDocument.Object);
        }

        [TestMethod]
        public async Task Creates_resource_collection_document_for_non_queryable_enumerables()
        {
            // Arrange
            var items = new[]
            {
                new Fruit {Id = "43", Name = "Strawberry"},
                new Fruit {Id = "43", Name = "Grape"}
            };

            var mockDocument = new Mock<IResourceCollectionDocument>(MockBehavior.Strict);

            var singleResourceDocumentBuilder = new Mock<ISingleResourceDocumentBuilder>(MockBehavior.Strict);

            var cancellationTokenSource = new CancellationTokenSource();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://www.example.com/fruits");

            var mockBaseUrlService = new Mock<IBaseUrlService>(MockBehavior.Strict);
            mockBaseUrlService.Setup(s => s.GetBaseUrl(request)).Returns("https://www.example.com/");
            
            var mockQueryableDocumentBuilder = new Mock<IQueryableResourceCollectionDocumentBuilder>(MockBehavior.Strict);
            var mockResourceCollectionDocumentBuilder = new Mock<IResourceCollectionDocumentBuilder>(MockBehavior.Strict);
            mockResourceCollectionDocumentBuilder
                .Setup(b => b.BuildDocument(items, "https://www.example.com/", It.IsAny<string[]>(), It.IsAny<IMetadata>()))
                .Returns(() => (mockDocument.Object));

            // Act
            var fallbackDocumentBuilder = new FallbackDocumentBuilder(singleResourceDocumentBuilder.Object,
                mockQueryableDocumentBuilder.Object, mockResourceCollectionDocumentBuilder.Object, mockBaseUrlService.Object);
            var resultDocument = await fallbackDocumentBuilder.BuildDocument(items, request, cancellationTokenSource.Token);

            // Assert
            resultDocument.Should().BeSameAs(mockDocument.Object);
        }
    }
}
