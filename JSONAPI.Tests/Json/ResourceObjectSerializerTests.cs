using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Json;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ResourceObjectSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_without_attributes()
        {
            IResourceObject resourceObject = new ResourceObject("countries", "1100");

            var serializer = new ResourceObjectSerializer(null, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_without_attributes.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_attributes()
        {
            var attributes = new Dictionary<string, JToken>
            {
                { "name", "Triangle" },
                { "sides", 3 },
                { "foo", null }
            };
            IResourceObject resourceObject = new ResourceObject("shapes", "1400", attributes);

            var serializer = new ResourceObjectSerializer(null, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_with_attributes.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_relationships()
        {
            var mockCapital = new Mock<IRelationshipObject>(MockBehavior.Strict);
            var mockNeighbors = new Mock<IRelationshipObject>(MockBehavior.Strict);

            var mockRelationshipObjectSerializer = new Mock<IRelationshipObjectSerializer>(MockBehavior.Strict);
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - capital");
                    return Task.FromResult(0);
                }).Verifiable();
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - neighbors");
                    return Task.FromResult(0);
                }).Verifiable();

            var relationships = new Dictionary<string, IRelationshipObject>
            {
                { "capital", mockCapital.Object },
                { "neighbors", mockNeighbors.Object }
            };
            IResourceObject resourceObject = new ResourceObject("states", "1400", relationships: relationships);

            var serializer = new ResourceObjectSerializer(mockRelationshipObjectSerializer.Object, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_with_relationships.json");
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_only_null_relationships()
        {
            var relationships = new Dictionary<string, IRelationshipObject>
            {
                { "capital", null }
            };
            IResourceObject resourceObject = new ResourceObject("states", "1400", relationships: relationships);

            var serializer = new ResourceObjectSerializer(null, null, null);
            await AssertSerializeOutput(serializer, resourceObject, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_with_only_null_relationships.json");
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_links()
        {
            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer.Setup(m => m.Serialize(It.IsAny<ILink>(), It.IsAny<JsonWriter>()))
                .Returns((ILink link, JsonWriter writer) =>
                {
                    writer.WriteValue("ILink placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockSelfLink = new Mock<ILink>(MockBehavior.Strict);

            IResourceObject resourceObject = new ResourceObject("states", "1400", selfLink: mockSelfLink.Object);

            var serializer = new ResourceObjectSerializer(null, mockLinkSerializer.Object, null);
            await AssertSerializeOutput(serializer, resourceObject, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_with_links.json");
            mockLinkSerializer.Verify(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_metadata()
        {
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("IMetadata placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            IResourceObject resourceObject = new ResourceObject("states", "1400", metadata: mockMetadata.Object);

            var serializer = new ResourceObjectSerializer(null, null, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, resourceObject, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_with_metadata.json");
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_all_possible_members()
        {
            var mockCapital = new Mock<IRelationshipObject>(MockBehavior.Strict);
            var mockNeighbors = new Mock<IRelationshipObject>(MockBehavior.Strict);

            var mockRelationshipObjectSerializer = new Mock<IRelationshipObjectSerializer>(MockBehavior.Strict);
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - capital");
                    return Task.FromResult(0);
                }).Verifiable();
            mockRelationshipObjectSerializer.Setup(m => m.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()))
                .Returns((IRelationshipObject relationshipObject, JsonWriter writer) =>
                {
                    writer.WriteValue("IRelationship Placeholder - neighbors");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer.Setup(m => m.Serialize(It.IsAny<ILink>(), It.IsAny<JsonWriter>()))
                .Returns((ILink link, JsonWriter writer) =>
                {
                    writer.WriteValue("ILink placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("IMetadata placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();


            var attributes = new Dictionary<string, JToken>
            {
                { "name", "New York" },
                { "population", 19746227 },
                { "foo", null }
            };

            var relationships = new Dictionary<string, IRelationshipObject>
            {
                { "capital", mockCapital.Object },
                { "neighbors", mockNeighbors.Object }
            };

            var mockSelfLink = new Mock<ILink>(MockBehavior.Strict);
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);

            IResourceObject resourceObject = new ResourceObject("states", "1400", attributes, relationships, mockSelfLink.Object, mockMetadata.Object);

            var serializer = new ResourceObjectSerializer(mockRelationshipObjectSerializer.Object, mockLinkSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, resourceObject, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_with_all_possible_members.json");
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockCapital.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockRelationshipObjectSerializer.Verify(s => s.Serialize(mockNeighbors.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockLinkSerializer.Verify(s => s.Serialize(mockSelfLink.Object, It.IsAny<JsonWriter>()), Times.Once);
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }

        class Sample
        {
            public string Id { get; set; }

            public UInt64 UInt64Field { get; set; }

            public UInt64? NullableUInt64Field { get; set; }
        }

        [TestMethod]
        public async Task Serialize_ResourceObject_for_resource_with_unsigned_long_integer_greater_than_int64_maxvalue()
        {
            var attributes = new Dictionary<string, JToken>
            {
                { "uInt64Field", 9223372036854775808 },
                { "nullableUInt64Field", 9223372036854775808 }
            };

            var resourceObject = new Mock<IResourceObject>();
            resourceObject.Setup(o => o.Id).Returns("2010");
            resourceObject.Setup(o => o.Type).Returns("samples");
            resourceObject.Setup(o => o.Attributes).Returns(attributes);
            resourceObject.Setup(o => o.Relationships).Returns(new Dictionary<string, IRelationshipObject>());

            var serializer = new ResourceObjectSerializer(null, null, null);
            await AssertSerializeOutput(serializer, resourceObject.Object, "Json/Fixtures/ResourceObjectSerializer/Serialize_ResourceObject_for_resource_with_unsigned_long_integer_greater_than_int64_maxvalue.json");
        }

        [TestMethod]
        public void Deserialize_resource_object()
        {
            // Arrange
            var mockAuthorRelationship = new Mock<IRelationshipObject>(MockBehavior.Strict);
            var mockCommentsRelationship = new Mock<IRelationshipObject>(MockBehavior.Strict);
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);

            var mockRelationshipSerializer = new Mock<IRelationshipObjectSerializer>(MockBehavior.Strict);
            mockRelationshipSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/relationships/author"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("AUTHOR_RELATIONSHIP");
                    return Task.FromResult(mockAuthorRelationship.Object);
                });
            mockRelationshipSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/relationships/comments"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("COMMENTS_RELATIONSHIP");
                    return Task.FromResult(mockCommentsRelationship.Object);
                });

            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), "/meta"))
                .Returns((JsonReader reader, string currentPath) =>
                {
                    reader.TokenType.Should().Be(JsonToken.String);
                    reader.Value.Should().Be("metadata goes here");
                    return Task.FromResult(mockMetadata.Object);
                });

            // Act
            var serializer = new ResourceObjectSerializer(mockRelationshipSerializer.Object, null, mockMetadataSerializer.Object);
            var resourceObject =
                GetDeserializedOutput<IResourceObjectSerializer, IResourceObject>(serializer,
                    "Json/Fixtures/ResourceObjectSerializer/Deserialize_resource_object.json").Result;

            // Assert
            resourceObject.Type.Should().Be("posts");
            resourceObject.Id.Should().Be("123456");

            resourceObject.Attributes.Keys.Count.Should().Be(3);
            resourceObject.Attributes["title"].Value<string>().Should().Be("Another awesome post");
            resourceObject.Attributes["likes"].Value<int>().Should().Be(43);
            resourceObject.Attributes["some-complex-attribute"].Should().BeOfType<JObject>();
            ((JObject) resourceObject.Attributes["some-complex-attribute"]).Properties().Count().Should().Be(1);
            ((int) ((JObject) resourceObject.Attributes["some-complex-attribute"])["qux"]).Should().Be(5);

            resourceObject.Relationships.Keys.Count.Should().Be(2);
            resourceObject.Relationships["author"].Should().BeSameAs(mockAuthorRelationship.Object);
            resourceObject.Relationships["comments"].Should().BeSameAs(mockCommentsRelationship.Object);

            resourceObject.Metadata.Should().BeSameAs(mockMetadata.Object);
        }
    }
}
