using System;
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
    public class MetadataSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_null_metadata()
        {
            var serializer = new MetadataSerializer();
            await AssertSerializeOutput(serializer, (IMetadata)null, "Json/Fixtures/MetadataSerializer/Serialize_null_metadata.json");
        }

        [TestMethod]
        public async Task Serialize_metadata()
        {
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            mockMetadata.Setup(m => m.MetaObject)
                .Returns(() =>
                {
                    var subObject = new JObject();
                    subObject["color"] = "yellow";
                    subObject["foo"] = 3;

                    var obj = new JObject();
                    obj["banana"] = subObject;
                    obj["bar"] = new DateTime(1776, 07, 04);
                    return obj;
                });

            var serializer = new MetadataSerializer();
            await AssertSerializeOutput(serializer, mockMetadata.Object, "Json/Fixtures/MetadataSerializer/Serialize_metadata.json");
        }

        [TestMethod]
        public void Serialize_metadata_should_fail_if_object_is_null()
        {
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            mockMetadata.Setup(m => m.MetaObject)
                .Returns(() => null);

            var serializer = new MetadataSerializer();

            Func<Task> action = async () =>
            {
                await
                    GetSerializedString(serializer, mockMetadata.Object);
            };
            action.ShouldThrow<JsonSerializationException>()
                .WithMessage("The meta object cannot be null.");
        }

        [TestMethod]
        public void Deserialize_null_metadata()
        {
            // Arrange

            // Act
            var serializer = new MetadataSerializer();
            var metadata =
                GetDeserializedOutput<IMetadataSerializer, IMetadata>(serializer,
                    "Json/Fixtures/MetadataSerializer/Deserialize_null_metadata.json").Result;

            // Assert
            metadata.Should().BeNull();
        }

        [TestMethod]
        public void Deserialize_metadata()
        {
            // Arrange

            // Act
            var serializer = new MetadataSerializer();
            var metadata =
                GetDeserializedOutput<IMetadataSerializer, IMetadata>(serializer,
                    "Json/Fixtures/MetadataSerializer/Deserialize_metadata.json").Result;

            // Assert
            ((int) metadata.MetaObject["foo"]).Should().Be(13);
            var baz = (JObject) metadata.MetaObject["baz"];
            ((string) baz["orange"]).Should().Be("qux");
        }
    }
}
