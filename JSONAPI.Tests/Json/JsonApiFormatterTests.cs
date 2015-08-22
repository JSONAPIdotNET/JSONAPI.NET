using System;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using JSONAPI.Json;
using System.IO;
using System.Net;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using Moq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class JsonApiFormatterTests
    {
        private JsonApiFormatter BuildFormatter(ISingleResourceDocumentFormatter singleResourceDocumentFormatter = null,
            IResourceCollectionDocumentFormatter resourceCollectionDocumentFormatter = null,
            IErrorDocumentFormatter errorDocumentFormatter = null,
            IErrorDocumentBuilder errorDocumentBuilder = null)
        {

            singleResourceDocumentFormatter = singleResourceDocumentFormatter ?? new Mock<ISingleResourceDocumentFormatter>(MockBehavior.Strict).Object;
            resourceCollectionDocumentFormatter = resourceCollectionDocumentFormatter ?? new Mock<IResourceCollectionDocumentFormatter>(MockBehavior.Strict).Object;
            errorDocumentFormatter = errorDocumentFormatter ?? new Mock<IErrorDocumentFormatter>(MockBehavior.Strict).Object;
            errorDocumentBuilder = errorDocumentBuilder ?? new ErrorDocumentBuilder();
            return new JsonApiFormatter(singleResourceDocumentFormatter, resourceCollectionDocumentFormatter, errorDocumentFormatter, errorDocumentBuilder);
        }

        [TestMethod]
        public void Serialize_SingleResourceDocument()
        {
            // Arrange
            var mockSingleResourceDocument = new Mock<ISingleResourceDocument>(MockBehavior.Strict);
            var singleResourceDocumentFormatter = new Mock<ISingleResourceDocumentFormatter>(MockBehavior.Strict);
            singleResourceDocumentFormatter.Setup(s => s.Serialize(mockSingleResourceDocument.Object, It.IsAny<JsonWriter>()))
                .Returns((ISingleResourceDocument p, JsonWriter writer) =>
                {
                    writer.WriteValue("SingleResourceDocument output goes here.");
                    return Task.FromResult(0);
                });

            var formatter = BuildFormatter(singleResourceDocumentFormatter.Object);
            var stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(mockSingleResourceDocument.Object.GetType(), mockSingleResourceDocument.Object, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_SingleResourceDocument.json");
        }

        [TestMethod]
        public void Serialize_ResourceCollectionDocument()
        {
            // Arrange
            var mockResourceCollectionDocument = new Mock<IResourceCollectionDocument>(MockBehavior.Strict);
            var resourceCollectionDocumentFormatter = new Mock<IResourceCollectionDocumentFormatter>(MockBehavior.Strict);
            resourceCollectionDocumentFormatter.Setup(s => s.Serialize(mockResourceCollectionDocument.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceCollectionDocument p, JsonWriter writer) =>
                {
                    writer.WriteValue("ResourceCollectionDocument output goes here.");
                    return Task.FromResult(0);
                });

            var formatter = BuildFormatter(resourceCollectionDocumentFormatter: resourceCollectionDocumentFormatter.Object);
            var stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(mockResourceCollectionDocument.Object.GetType(), mockResourceCollectionDocument.Object, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_ResourceCollectionDocument.json");
        }

        [TestMethod]
        public void Serialize_ErrorDocument()
        {
            // Arrange
            var errorDocument = new Mock<IErrorDocument>(MockBehavior.Strict);
            var errorDocumentFormatter = new Mock<IErrorDocumentFormatter>(MockBehavior.Strict);
            errorDocumentFormatter.Setup(s => s.Serialize(errorDocument.Object, It.IsAny<JsonWriter>()))
                .Returns((IErrorDocument p, JsonWriter writer) =>
                {
                    writer.WriteValue("ErrorDocument output goes here.");
                    return Task.FromResult(0);
                });

            var formatter = BuildFormatter(errorDocumentFormatter: errorDocumentFormatter.Object);
            var stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(errorDocument.Object.GetType(), errorDocument.Object, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_ErrorDocument.json");
        }

        [TestMethod]
        public void Serialize_HttpError()
        {
            // Arrange
            var httpError = new HttpError(new Exception("This is the exception message"), true);
            var mockErrorDocumentBuilder = new Mock<IErrorDocumentBuilder>(MockBehavior.Strict);
            var mockErrorDocument = new Mock<IErrorDocument>(MockBehavior.Strict);
            mockErrorDocumentBuilder.Setup(b => b.BuildFromHttpError(httpError, HttpStatusCode.InternalServerError))
                .Returns(mockErrorDocument.Object);

            var mockErrorDocumentFormatter = new Mock<IErrorDocumentFormatter>(MockBehavior.Strict);
            mockErrorDocumentFormatter.Setup(s => s.Serialize(mockErrorDocument.Object, It.IsAny<JsonWriter>()))
                .Returns((IErrorDocument errorDocument, JsonWriter writer) =>
                {
                    writer.WriteValue("HttpError document");
                    return Task.FromResult(0);
                });

            var stream = new MemoryStream();

            // Act
            var formatter = BuildFormatter(errorDocumentBuilder: mockErrorDocumentBuilder.Object, errorDocumentFormatter: mockErrorDocumentFormatter.Object);
            formatter.WriteToStreamAsync(httpError.GetType(), httpError, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_HttpError.json");
        }

        private class Color
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        [TestMethod]
        public void Writes_error_for_anything_else()
        {
            // Arrange
            var formatter = BuildFormatter();
            var stream = new MemoryStream();

            // Act
            var resource = new Color { Id = "1", Name = "Blue" };
            formatter.WriteToStreamAsync(resource.GetType(), resource, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Writes_error_for_anything_else.json");
        }

        [TestMethod]
        public void ReadFromStreamAsync_deserializes_ISingleResourceDocument()
        {
            // Arrange
            var mockSingleResourceDocument = new Mock<ISingleResourceDocument>(MockBehavior.Strict);
            var singleResourceDocumentFormatter = new Mock<ISingleResourceDocumentFormatter>(MockBehavior.Strict);
            singleResourceDocumentFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), ""))
                .Returns(Task.FromResult(mockSingleResourceDocument.Object));

            var formatter = BuildFormatter(singleResourceDocumentFormatter.Object);
            var stream = new MemoryStream();

            // Act
            var deserialized = formatter.ReadFromStreamAsync(typeof(ISingleResourceDocument), stream, null, null).Result;

            // Assert
            deserialized.Should().BeSameAs(mockSingleResourceDocument.Object);
        }

        [TestMethod]
        public void ReadFromStreamAsync_deserializes_IResourceCollectionDocument()
        {
            // Arrange
            var mockResourceCollectionDocument = new Mock<IResourceCollectionDocument>(MockBehavior.Strict);
            var resourceCollectionDocumentFormatter = new Mock<IResourceCollectionDocumentFormatter>(MockBehavior.Strict);
            resourceCollectionDocumentFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), ""))
                .Returns(Task.FromResult(mockResourceCollectionDocument.Object));

            var formatter = BuildFormatter(resourceCollectionDocumentFormatter: resourceCollectionDocumentFormatter.Object);
            var stream = new MemoryStream();

            // Act
            var deserialized = formatter.ReadFromStreamAsync(typeof(IResourceCollectionDocument), stream, null, null).Result;

            // Assert
            deserialized.Should().BeSameAs(mockResourceCollectionDocument.Object);
        }
    }
}
