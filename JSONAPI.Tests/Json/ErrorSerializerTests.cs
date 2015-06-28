using System.Net;
using System.Threading.Tasks;
using JSONAPI.Json;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ErrorSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_error_with_only_id()
        {
            var error = new Mock<IError>();
            error.Setup(e => e.Id).Returns("123456");

            var serializer = new ErrorSerializer(null, null);
            await AssertSerializeOutput(serializer, error.Object, "Json/Fixtures/ErrorSerializer/Serialize_error_with_only_id.json");
        }

        [TestMethod]
        public async Task Serialize_error_with_all_possible_members()
        {
            var mockAboutLink = new Mock<ILink>(MockBehavior.Strict);
            mockAboutLink.Setup(l => l.Href).Returns("http://example.com/my-about-link");

            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            mockMetadata.Setup(m => m.MetaObject).Returns(() =>
            {
                var obj = new JObject();
                obj["foo"] = "qux";
                return obj;
            });

            var error = new Mock<IError>(MockBehavior.Strict);
            error.Setup(e => e.Id).Returns("654321");
            error.Setup(e => e.AboutLink).Returns(mockAboutLink.Object);
            error.Setup(e => e.Status).Returns(HttpStatusCode.BadRequest);
            error.Setup(e => e.Code).Returns("9000");
            error.Setup(e => e.Title).Returns("Some error occurred.");
            error.Setup(e => e.Detail).Returns("The thingamabob fell through the whatsit.");
            error.Setup(e => e.Pointer).Returns("/data/attributes/bob");
            error.Setup(e => e.Parameter).Returns("sort");
            error.Setup(e => e.Metadata).Returns(mockMetadata.Object);

            var mockLinkSerializer = new Mock<ILinkSerializer>(MockBehavior.Strict);
            mockLinkSerializer.Setup(s => s.Serialize(mockAboutLink.Object, It.IsAny<JsonWriter>()))
                .Returns((ILink link, JsonWriter writer) =>
                {
                    writer.WriteValue(link.Href);
                    return Task.FromResult(0);
                });

            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    metadata.MetaObject.WriteTo(writer);
                    return Task.FromResult(0);
                });

            var serializer = new ErrorSerializer(mockLinkSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, error.Object, "Json/Fixtures/ErrorSerializer/Serialize_error_with_all_possible_members.json");
        }
    }
}
