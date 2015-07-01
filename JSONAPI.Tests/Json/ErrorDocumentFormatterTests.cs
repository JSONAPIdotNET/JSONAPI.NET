using System.Threading.Tasks;
using JSONAPI.Documents;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ErrorDocumentFormatterTests : JsonApiFormatterTestsBase
    {
        [TestMethod]
        public async Task Serialize_ErrorDocument()
        {
            var error1 = new Mock<IError>(MockBehavior.Strict);
            var error2 = new Mock<IError>(MockBehavior.Strict);

            var mockErrorFormatter = new Mock<IErrorFormatter>(MockBehavior.Strict);
            mockErrorFormatter.Setup(s => s.Serialize(error1.Object, It.IsAny<JsonWriter>()))
                .Returns((IError error, JsonWriter writer) =>
                {
                    writer.WriteValue("first error would go here");
                    return Task.FromResult(0);
                });
            mockErrorFormatter.Setup(s => s.Serialize(error2.Object, It.IsAny<JsonWriter>()))
                .Returns((IError error, JsonWriter writer) =>
                {
                    writer.WriteValue("second error would go here");
                    return Task.FromResult(0);
                });
            
            var mockMetadata = new Mock<IMetadata>(MockBehavior.Strict);
            var mockMetadataFormatter = new Mock<IMetadataFormatter>(MockBehavior.Strict);
            mockMetadataFormatter.Setup(s => s.Serialize(mockMetadata.Object, It.IsAny<JsonWriter>()))
                .Returns((IMetadata metadata, JsonWriter writer) =>
                {
                    writer.WriteValue("metadata goes here");
                    return Task.FromResult(0);
                });

            IErrorDocument document = new ErrorDocument(new[] { error1.Object, error2.Object }, mockMetadata.Object);

            var formatter = new ErrorDocumentFormatter(mockErrorFormatter.Object, mockMetadataFormatter.Object);
            await AssertSerializeOutput(formatter, document, "Json/Fixtures/ErrorDocumentFormatter/Serialize_ErrorDocument.json");
        }
    }
}
