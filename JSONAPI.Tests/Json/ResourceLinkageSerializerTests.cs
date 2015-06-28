using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Json;
using JSONAPI.Payload;
using JSONAPI.Tests.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ResourceLinkageSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.LinkageToken).Returns("linkage goes here");

            var serializer = new ResourceLinkageSerializer();
            await AssertSerializeOutput(serializer, linkageObject.Object, "Json/Fixtures/ResourceLinkageSerializer/Serialize_linkage.json");
        }

        [TestMethod]
        public async Task Serialize_null_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.LinkageToken).Returns((JToken)null);

            var serializer = new ResourceLinkageSerializer();
            await AssertSerializeOutput(serializer, linkageObject.Object, "Json/Fixtures/ResourceLinkageSerializer/Serialize_null_linkage.json");
        }

        [TestMethod]
        public void Deserialize_to_one_linkage()
        {
            // Arrange

            // Act
            var serializer = new ResourceLinkageSerializer();
            var linkage =
                GetDeserializedOutput<IResourceLinkageSerializer, IResourceLinkage>(serializer,
                    "Json/Fixtures/ResourceLinkageSerializer/Deserialize_to_one_linkage.json").Result;

            // Assert
            var linkageToken = (JObject)linkage.LinkageToken;
            linkageToken.Properties().Count().Should().Be(2);
            ((string)linkageToken["type"]).Should().Be("people");
            ((string)linkageToken["id"]).Should().Be("abc123");
        }

        [TestMethod]
        public void Deserialize_null_to_one_linkage()
        {
            // Arrange

            // Act
            var serializer = new ResourceLinkageSerializer();
            var linkage =
                GetDeserializedOutput<IResourceLinkageSerializer, IResourceLinkage>(serializer,
                    "Json/Fixtures/ResourceLinkageSerializer/Deserialize_null_to_one_linkage.json").Result;

            // Assert
            linkage.LinkageToken.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_to_many_linkage()
        {
            // Arrange

            // Act
            var serializer = new ResourceLinkageSerializer();
            var linkage =
                GetDeserializedOutput<IResourceLinkageSerializer, IResourceLinkage>(serializer,
                    "Json/Fixtures/ResourceLinkageSerializer/Deserialize_to_many_linkage.json").Result;

            // Assert
            var linkageToken = (JArray)linkage.LinkageToken;

            var item1 = (JObject) linkageToken[0];
            item1.Properties().Count().Should().Be(2);
            ((string)item1["type"]).Should().Be("posts");
            ((string)item1["id"]).Should().Be("12");

            var item2 = (JObject)linkageToken[1];
            item2.Properties().Count().Should().Be(2);
            ((string)item2["type"]).Should().Be("comments");
            ((string)item2["id"]).Should().Be("9510");
        }

        [TestMethod]
        public void Deserialize_fails_on_string()
        {
            // Arrange

            // Act
            var serializer = new ResourceLinkageSerializer();

            Func<Task> action = () => GetDeserializedOutput<IResourceLinkageSerializer, IResourceLinkage>(serializer,
                "Json/Fixtures/ResourceLinkageSerializer/Deserialize_fails_on_string.json");

            // Assert
            action.ShouldThrow<DeserializationException>().WithMessage("Expected an array, object, or null for linkage, but got String");
        }

        [TestMethod]
        public void Deserialize_fails_on_integer()
        {
            // Arrange

            // Act
            var serializer = new ResourceLinkageSerializer();

            Func<Task> action = () => GetDeserializedOutput<IResourceLinkageSerializer, IResourceLinkage>(serializer,
                "Json/Fixtures/ResourceLinkageSerializer/Deserialize_fails_on_integer.json");

            // Assert
            action.ShouldThrow<DeserializationException>().WithMessage("Expected an array, object, or null for linkage, but got Integer");
        }
    }
}
