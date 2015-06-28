using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Json;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ResourceCollectionPayloadSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_ResourceCollectionPayload_for_primary_data_only()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            IResourceCollectionPayload payload = new ResourceCollectionPayload(primaryData, null, null);

            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, null);
            await AssertSerializeOutput(serializer, payload, "Json/Fixtures/ResourceCollectionPayloadSerializer/Serialize_ResourceCollectionPayload_for_primary_data_only.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceCollectionPayload_for_primary_data_only_and_metadata()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            IResourceCollectionPayload payload = new ResourceCollectionPayload(primaryData, null, mockMetadata.Object);

            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, payload, "Json/Fixtures/ResourceCollectionPayloadSerializer/Serialize_ResourceCollectionPayload_for_primary_data_only_and_metadata.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceCollectionPayload_for_all_possible_members()
        {
            var primaryData1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var primaryData2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource2 = new Mock<IResourceObject>(MockBehavior.Strict);
            var relatedResource3 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(primaryData2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Primary data 2");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource1.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 1");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource2.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 2");
                    return Task.FromResult(0);
                });
            mockResourceObjectSerializer.Setup(m => m.Serialize(relatedResource3.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceObject resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Related data object 3");
                    return Task.FromResult(0);
                });

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata resourceObject, JsonWriter writer) =>
                {
                    writer.WriteValue("Placeholder metadata object");
                    return Task.FromResult(0);
                });

            var primaryData = new[] { primaryData1.Object, primaryData2.Object };
            var relatedResources = new[] { relatedResource1.Object, relatedResource2.Object, relatedResource3.Object };
            IResourceCollectionPayload payload = new ResourceCollectionPayload(primaryData, relatedResources, mockMetadata.Object);

            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, payload, "Json/Fixtures/ResourceCollectionPayloadSerializer/Serialize_ResourceCollectionPayload_for_all_possible_members.json");
        }

        [TestMethod]
        public void Deserialize_empty_payload()
        {
            // Arrange
            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);

            // Act
            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            var payload =
                GetDeserializedOutput<IResourceCollectionPayloadSerializer, IResourceCollectionPayload>(serializer,
                    "Json/Fixtures/ResourceCollectionPayloadSerializer/Deserialize_empty_payload.json").Result;

            // Assert
            payload.PrimaryData.Should().BeEquivalentTo();
            payload.RelatedData.Should().BeEquivalentTo();
            payload.Metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_payload_with_primary_data()
        {
            // Arrange
            var mockResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResource2 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/0"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD1");
                    return Task.FromResult(mockResource1.Object);
                });
            mockResourceObjectSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/1"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD2");
                    return Task.FromResult(mockResource2.Object);
                });
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);

            // Act
            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            var payload =
                GetDeserializedOutput<IResourceCollectionPayloadSerializer, IResourceCollectionPayload>(serializer,
                    "Json/Fixtures/ResourceCollectionPayloadSerializer/Deserialize_payload_with_primary_data.json").Result;

            // Assert
            payload.PrimaryData.Should().BeEquivalentTo(mockResource1.Object, mockResource2.Object);
            payload.RelatedData.Should().BeEquivalentTo();
            payload.Metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_payload_with_primary_data_and_unknown_top_level_key()
        {
            // Arrange
            var mockResource1 = new Mock<IResourceObject>(MockBehavior.Strict);
            var mockResource2 = new Mock<IResourceObject>(MockBehavior.Strict);

            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            mockResourceObjectSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/0"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD1");
                    return Task.FromResult(mockResource1.Object);
                });
            mockResourceObjectSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/data/1"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("PD2");
                    return Task.FromResult(mockResource2.Object);
                });
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);

            // Act
            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            var payload =
                GetDeserializedOutput<IResourceCollectionPayloadSerializer, IResourceCollectionPayload>(serializer,
                    "Json/Fixtures/ResourceCollectionPayloadSerializer/Deserialize_payload_with_primary_data_and_unknown_top_level_key.json").Result;

            // Assert
            payload.PrimaryData.Should().BeEquivalentTo(mockResource1.Object, mockResource2.Object);
            payload.RelatedData.Should().BeEquivalentTo();
            payload.Metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_payload_with_metadata()
        {
            // Arrange
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);

            var mockResourceObjectSerializer = new Mock<IResourceObjectSerializer>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/meta"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("metadata goes here");
                    return Task.FromResult(mockMetadata.Object);
                });

            // Act
            var serializer = new ResourceCollectionPayloadSerializer(mockResourceObjectSerializer.Object, mockMetadataSerializer.Object);
            var payload =
                GetDeserializedOutput<IResourceCollectionPayloadSerializer, IResourceCollectionPayload>(serializer,
                    "Json/Fixtures/ResourceCollectionPayloadSerializer/Deserialize_payload_with_metadata.json").Result;

            // Assert
            payload.PrimaryData.Should().BeEquivalentTo();
            payload.RelatedData.Should().BeEquivalentTo();
            payload.Metadata.Should().BeSameAs(mockMetadata.Object);
        }
    }
}
