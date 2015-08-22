using System.Threading.Tasks;
using JSONAPI.Documents;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class LinkFormatterTests : JsonApiFormatterTestsBase
    {
        [TestMethod]
        public async Task Serialize_link_without_metadata()
        {
            ILink link = new Link("http://www.example.com", null);

            var formatter = new LinkFormatter(null);
            await AssertSerializeOutput(formatter, link, "Json/Fixtures/LinkFormatter/Serialize_link_without_metadata.json");
        }

        [TestMethod]
        public async Task Serialize_link_with_metadata()
        {
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);
            mockMetadataFormatter.Setup(m => m.Serialize(It.IsAny<IMetadata>(), It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("IMetadata placeholder 1");
                    return Task.FromResult(0);
                }).Verifiable();

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            ILink link = new Link("http://www.example.com", mockMetadata.Object);

            var formatter = new LinkFormatter(mockMetadataFormatter.Object);
            await AssertSerializeOutput(formatter, link, "Json/Fixtures/LinkFormatter/Serialize_link_with_metadata.json");
            mockMetadataFormatter.Verify(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()), Times.Once);
        }
    }
}
