using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Documents;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ResourceLinkageFormatterTests : JsonApiFormatterTestsBase
    {
        [TestMethod]
        public async Task Serialize_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.LinkageToken).Returns("linkage goes here");

            var formatter = new ResourceLinkageFormatter();
            await AssertSerializeOutput(formatter, linkageObject.Object, "Json/Fixtures/ResourceLinkageFormatter/Serialize_linkage.json");
        }

        [TestMethod]
        public async Task Serialize_null_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.LinkageToken).Returns((JToken)null);

            var formatter = new ResourceLinkageFormatter();
            await AssertSerializeOutput(formatter, linkageObject.Object, "Json/Fixtures/ResourceLinkageFormatter/Serialize_null_linkage.json");
        }

        [TestMethod]
        public void Deserialize_to_one_linkage()
        {
            // Arrange

            // Act
            var formatter = new ResourceLinkageFormatter();
            var linkage =
                GetDeserializedOutput<IResourceLinkageFormatter, IResourceLinkage>(formatter,
                    "Json/Fixtures/ResourceLinkageFormatter/Deserialize_to_one_linkage.json").Result;

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
            var formatter = new ResourceLinkageFormatter();
            var linkage =
                GetDeserializedOutput<IResourceLinkageFormatter, IResourceLinkage>(formatter,
                    "Json/Fixtures/ResourceLinkageFormatter/Deserialize_null_to_one_linkage.json").Result;

            // Assert
            linkage.LinkageToken.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_to_many_linkage()
        {
            // Arrange

            // Act
            var formatter = new ResourceLinkageFormatter();
            var linkage =
                GetDeserializedOutput<IResourceLinkageFormatter, IResourceLinkage>(formatter,
                    "Json/Fixtures/ResourceLinkageFormatter/Deserialize_to_many_linkage.json").Result;

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
            var formatter = new ResourceLinkageFormatter();

            Func<Task> action = () => GetDeserializedOutput<IResourceLinkageFormatter, IResourceLinkage>(formatter,
                "Json/Fixtures/ResourceLinkageFormatter/Deserialize_fails_on_string.json");

            // Assert
            action.ShouldThrow<DeserializationException>().WithMessage("Expected an array, object, or null for linkage, but got String");
        }

        [TestMethod]
        public void Deserialize_fails_on_integer()
        {
            // Arrange

            // Act
            var formatter = new ResourceLinkageFormatter();

            Func<Task> action = () => GetDeserializedOutput<IResourceLinkageFormatter, IResourceLinkage>(formatter,
                "Json/Fixtures/ResourceLinkageFormatter/Deserialize_fails_on_integer.json");

            // Assert
            action.ShouldThrow<DeserializationException>().WithMessage("Expected an array, object, or null for linkage, but got Integer");
        }
    }
}
