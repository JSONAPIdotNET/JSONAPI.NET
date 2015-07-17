using System;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.Documents;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ResourceLinkageFormatterTests : JsonApiFormatterTestsBase
    {
        [TestMethod]
        public async Task Serialize_present_toMany_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.IsToMany).Returns(true);
            linkageObject.Setup(l => l.Identifiers)
                .Returns(new IResourceIdentifier[] { new ResourceIdentifier("countries", "11000"), new ResourceIdentifier("cities", "4100") });

            var formatter = new ResourceLinkageFormatter();
            await AssertSerializeOutput(formatter, linkageObject.Object, "Json/Fixtures/ResourceLinkageFormatter/Serialize_present_toMany_linkage.json");
        }

        [TestMethod]
        public async Task Serialize_empty_toMany_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.IsToMany).Returns(true);
            linkageObject.Setup(l => l.Identifiers).Returns(new IResourceIdentifier[] { });

            var formatter = new ResourceLinkageFormatter();
            await AssertSerializeOutput(formatter, linkageObject.Object, "Json/Fixtures/ResourceLinkageFormatter/Serialize_empty_toMany_linkage.json");
        }

        [TestMethod]
        public async Task Serialize_present_toOne_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.IsToMany).Returns(false);
            linkageObject.Setup(l => l.Identifiers).Returns(new IResourceIdentifier[] { new ResourceIdentifier("countries", "11000") });

            var formatter = new ResourceLinkageFormatter();
            await AssertSerializeOutput(formatter, linkageObject.Object, "Json/Fixtures/ResourceLinkageFormatter/Serialize_present_toOne_linkage.json");
        }

        [TestMethod]
        public async Task Serialize_null_toOne_linkage()
        {
            var linkageObject = new Mock<IResourceLinkage>(MockBehavior.Strict);
            linkageObject.Setup(l => l.IsToMany).Returns(false);
            linkageObject.Setup(l => l.Identifiers).Returns(new IResourceIdentifier[] { });

            var formatter = new ResourceLinkageFormatter();
            await AssertSerializeOutput(formatter, linkageObject.Object, "Json/Fixtures/ResourceLinkageFormatter/Serialize_null_toOne_linkage.json");
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
            linkage.IsToMany.Should().BeFalse();
            linkage.Identifiers.Length.Should().Be(1);
            linkage.Identifiers[0].Type.Should().Be("people");
            linkage.Identifiers[0].Id.Should().Be("abc123");
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
            linkage.IsToMany.Should().BeFalse();
            linkage.Identifiers.Length.Should().Be(0);
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
            linkage.IsToMany.Should().BeTrue();
            linkage.Identifiers[0].Type.Should().Be("posts");
            linkage.Identifiers[0].Id.Should().Be("12");
            linkage.Identifiers[1].Type.Should().Be("comments");
            linkage.Identifiers[1].Id.Should().Be("9510");
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
