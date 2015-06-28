using System.Threading.Tasks;
using JSONAPI.Json;
using JSONAPI.Payload;
using JSONAPI.Tests.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class LinkSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_link_without_metadata()
        {
            ILink link = new Link("http://www.example.com", null);

            var serializer = new LinkSerializer(null);
            await AssertSerializeOutput(serializer, link, "Json/Fixtures/LinkSerializer/Serialize_link_without_metadata.json");
        }

        [TestMethod]
        public async Task Serialize_link_with_metadata()
        {
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("IMetadata placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            ILink link = new Link("http://www.example.com", mockMetadata.Object);

            var serializer = new LinkSerializer(mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, link, "Json/Fixtures/LinkSerializer/Serialize_link_with_metadata.json");
            mockMetadataSerializer.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }
    }
}
