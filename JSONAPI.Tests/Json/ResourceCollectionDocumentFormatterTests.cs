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
    public class ResourceCollectionDocumentFormatterTests : JsonApiFormatterTestsBase
    {
        [TestMethod]
        public async Task Serialize_ResourceCollectionDocument_for_primary_data_only()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectFormatter.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            IResourceCollectionDocument document = new ResourceCollectionDocument(primaryData, null, null);

            var formatter = new ResourceCollectionDocumentFormatter(mockResourceObjectFormatter.Object, null);
            await AssertSerializeOutput(formatter, document, "Json/Fixtures/ResourceCollectionDocumentFormatter/Serialize_ResourceCollectionDocument_for_primary_data_only.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceCollectionDocument_for_primary_data_only_and_metadata()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectFormatter.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);
            mockMetadataFormatter.Setup(m => m.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            IResourceCollectionDocument document = new ResourceCollectionDocument(primaryData, null, mockMetadata.Object);

            var formatter = new ResourceCollectionDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            await AssertSerializeOutput(formatter, document, "Json/Fixtures/ResourceCollectionDocumentFormatter/Serialize_ResourceCollectionDocument_for_primary_data_only_and_metadata.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceCollectionDocument_for_all_possible_members()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource3 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectFormatter.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });
            mockResourceObjectFormatter.Setup(m => m.Serialize(relatedResource1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectFormatter.Setup(m => m.Serialize(relatedResource2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 2");
                    return Task.FromResult(0);
                });
            mockResourceObjectFormatter.Setup(m => m.Serialize(relatedResource3.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 3");
                    return Task.FromResult(0);
                });

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);
            mockMetadataFormatter.Setup(m => m.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            var relatedResources = new[] { relatedResource1.Object, relatedResource2.Object, relatedResource3.Object };
            IResourceCollectionDocument document = new ResourceCollectionDocument(primaryData, relatedResources, mockMetadata.Object);

            var formatter = new ResourceCollectionDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            await AssertSerializeOutput(formatter, document, "Json/Fixtures/ResourceCollectionDocumentFormatter/Serialize_ResourceCollectionDocument_for_all_possible_members.json");
        }

        [TestMethod]
        public void Deserialize_empty_document()
        {
            // Arrange
            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);

            // Act
            var formatter = new ResourceCollectionDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            var document =
                GetDeserializedOutput<IResourceCollectionDocumentFormatter, IResourceCollectionDocument>(formatter,
                    "Json/Fixtures/ResourceCollectionDocumentFormatter/Deserialize_empty_document.json").Result;

            // Assert
            document.PrimaryData.Should().BeEquivalentTo();
            document.RelatedData.Should().BeEquivalentTo();
            document.Metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_document_with_primary_data()
        {
            // Arrange
            var mockResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResource2 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/0"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD1");
                    return Task.FromResult(mockResource1.Object);
                });
            mockResourceObjectFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/1"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD2");
                    return Task.FromResult(mockResource2.Object);
                });
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);

            // Act
            var formatter = new ResourceCollectionDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            var document =
                GetDeserializedOutput<IResourceCollectionDocumentFormatter, IResourceCollectionDocument>(formatter,
                    "Json/Fixtures/ResourceCollectionDocumentFormatter/Deserialize_document_with_primary_data.json").Result;

            // Assert
            document.PrimaryData.Should().BeEquivalentTo(mockResource1.Object, mockResource2.Object);
            document.RelatedData.Should().BeEquivalentTo();
            document.Metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_document_with_primary_data_and_unknown_top_level_key()
        {
            // Arrange
            var mockResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResource2 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            mockResourceObjectFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/0"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD1");
                    return Task.FromResult(mockResource1.Object);
                });
            mockResourceObjectFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/1"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD2");
                    return Task.FromResult(mockResource2.Object);
                });
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);

            // Act
            var formatter = new ResourceCollectionDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            var document =
                GetDeserializedOutput<IResourceCollectionDocumentFormatter, IResourceCollectionDocument>(formatter,
                    "Json/Fixtures/ResourceCollectionDocumentFormatter/Deserialize_document_with_primary_data_and_unknown_top_level_key.json").Result;

            // Assert
            document.PrimaryData.Should().BeEquivalentTo(mockResource1.Object, mockResource2.Object);
            document.RelatedData.Should().BeEquivalentTo();
            document.Metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_document_with_metadata()
        {
            // Arrange
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);

            var mockResourceObjectFormatter = new Mock<IResourceObjectFormatter>(MockBehavior.Strict);
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);
            mockMetadataFormatter.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/meta"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("metadata goes here");
                    return Task.FromResult(mockMetadata.Object);
                });

            // Act
            var formatter = new ResourceCollectionDocumentFormatter(mockResourceObjectFormatter.Object, mockMetadataFormatter.Object);
            var document =
                GetDeserializedOutput<IResourceCollectionDocumentFormatter, IResourceCollectionDocument>(formatter,
                    "Json/Fixtures/ResourceCollectionDocumentFormatter/Deserialize_document_with_metadata.json").Result;

            // Assert
            document.PrimaryData.Should().BeEquivalentTo();
            document.RelatedData.Should().BeEquivalentTo();
            document.Metadata.Should().BeSameAs(mockMetadata.Object);
        }
    }
}
