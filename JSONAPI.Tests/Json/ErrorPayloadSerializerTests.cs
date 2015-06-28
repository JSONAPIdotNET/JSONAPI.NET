using System.Threading.Tasks;
using JSONAPI.Json;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ErrorPayloadSerializerTests : JsonApiSerializerTestsBase
    {
        [TestMethod]
        public async Task Serialize_ErrorPayload()
        {
            var error1 = new Mock<IError>(MockBehavior.Strict);
            var error2 = new Mock<IError>(MockBehavior.Strict);

            var mockErrorSerializer = new Mock<IErrorSerializer>(MockBehavior.Strict);
            mockErrorSerializer.Setup(s => s.Serialize(error1.Object, It.IsAny<JsonWriter>()))
                .Returns((IError error, JsonWriter writer) =>
                {
                    writer.WriteValue("first error would go here");
                    return Task.FromResult(0);
                });
            mockErrorSerializer.Setup(s => s.Serialize(error2.Object, It.IsAny<JsonWriter>()))
                .Returns((IError error, JsonWriter writer) =>
                {
                    writer.WriteValue("second error would go here");
                    return Task.FromResult(0);
                });
            
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataSerializer = new Mock<IMetadataSerializer>(MockBehavior.Strict);
            mockMetadataSerializer.Setup(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("metadata goes here");
                    return Task.FromResult(0);
                });

            IErrorPayload payload = new ErrorPayload(new[] { error1.Object, error2.Object }, mockMetadata.Object);

            var serializer = new ErrorPayloadSerializer(mockErrorSerializer.Object, mockMetadataSerializer.Object);
            await AssertSerializeOutput(serializer, payload, "Json/Fixtures/ErrorPayloadSerializer/Serialize_ErrorPayload.json");
        }
    }
}
