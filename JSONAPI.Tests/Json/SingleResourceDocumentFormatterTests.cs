using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Documents;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class SingleResourceDocumentFormatterTests : JsonApiFormatterTestsBase
    {
        [TestMethod]
        public async Task Serialize_SingleResourceDocument_for_primary_data_only()
        {
            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(m => m.Serialize(It.IsAny<IResourceObject>(), It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder resource object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockResource = new Mock<IResourceObject>(MockBehavior.Strict);
            ISingleResourceDocument document = new SingleResourceDocument(mockResource.Object, null, null);

            var formatter = new SingleResourceDocumentFormatter(mockResourceObjectFormatter.Object, null);
            await AssertSerializeOutput(formatter, document, "Json/Fixtures/SingleResourceDocumentFormatter/Serialize_SingleResourceDocument_for_primary_data_only.json");
            mockResourceObjectFormatter.Verify(s => s.Serialize(mockResource.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_SingleResourceDocument_for_primary_data_and_metadata()
        {
            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(m => m.Serialize(It.IsAny<IResourceObject>(), It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder resource object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);
            mockMetadataFormatter.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockResource = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            ISingleResourceDocument document = new SingleResourceDocument(mockResource.Object, null, mockMetadata.Object);

            var formatter = new SingleResourceDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            await AssertSerializeOutput(formatter, document, "Json/Fixtures/SingleResourceDocumentFormatter/Serialize_SingleResourceDocument_for_primary_data_and_metadata.json");
            mockResourceObjectFormatter.Verify(s => s.Serialize(mockResource.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockMetadataFormatter.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_SingleResourceDocument_for_all_possible_members()
        {
            var mockPrimaryData = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource3 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(m => m.Serialize(mockPrimaryData.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data object");
                    return Task.FromResult(0);
                }).Verifiable();
            mockResourceObjectFormatter.Setup(m => m.Serialize(relatedResource1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 1");
                    return Task.FromResult(0);
                }).Verifiable();
            mockResourceObjectFormatter.Setup(m => m.Serialize(relatedResource2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 2");
                    return Task.FromResult(0);
                }).Verifiable();
            mockResourceObjectFormatter.Setup(m => m.Serialize(relatedResource3.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 3");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);
            mockMetadataFormatter.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var relatedResources = new[] { relatedResource1.Object, relatedResource2.Object, relatedResource3.Object };
            ISingleResourceDocument document = new SingleResourceDocument(mockPrimaryData.Object, relatedResources, mockMetadata.Object);

            var formatter = new SingleResourceDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            await AssertSerializeOutput(formatter, document, "Json/Fixtures/SingleResourceDocumentFormatter/Serialize_SingleResourceDocument_for_all_possible_members.json");
            mockResourceObjectFormatter.Verify(s => s.Serialize(mockPrimaryData.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockResourceObjectFormatter.Verify(s => s.Serialize(relatedResource1.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockResourceObjectFormatter.Verify(s => s.Serialize(relatedResource2.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockResourceObjectFormatter.Verify(s => s.Serialize(relatedResource3.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockMetadataFormatter.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public void Deserialize_null_document()
        {
            // Arrange
            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);

            // Act
            var formatter = new SingleResourceDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            var document =
                GetDeserializedOutput<ISingleResourceDocumentFormatter, ISingleResourceDocument>(formatter,
                    "Json/Fixtures/SingleResourceDocumentFormatter/Deserialize_null_document.json").Result;

            // Assert
            document.PrimaryData.Should().BeNull();
            document.RelatedData.Should().BeEquivalentTo();
            document.Metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_document_with_resource()
        {
            // Arrange
            var mockResourceObject = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("primary data goes here");
                    return Task.FromResult(mockResourceObject.Object);
                });
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);

            // Act
            var formatter = new SingleResourceDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            var document =
                GetDeserializedOutput<ISingleResourceDocumentFormatter, ISingleResourceDocument>(formatter,
                    "Json/Fixtures/SingleResourceDocumentFormatter/Deserialize_document_with_resource.json").Result;

            // Assert
            document.PrimaryData.Should().BeSameAs(mockResourceObject.Object);
            document.RelatedData.Should().BeEquivalentTo();
            document.Metadata.Should().BeNull();
        }
    }
}
