using System;
using System.IO;
using System.Web.Http;
using FluentAssertions;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ErrorSerializerTests
    {
        [TestMethod]
        public void CanSerialize_returns_true_for_HttpError()
        {
            var serializer = new ErrorSerializer();
            var result = serializer.CanSerialize(typeof (HttpError));
            result.Should().BeTrue();
        }

        [TestMethod]
        public void CanSerialize_returns_false_for_Exception()
        {
            var serializer = new ErrorSerializer();
            var result = serializer.CanSerialize(typeof(Exception));
            result.Should().BeFalse();
        }

        [TestMethod]
        [DeploymentItem(@"Data\ErrorSerializerTest.json")]
        public void SerializeError_serializes_httperror()
        {
            using (var stream = new MemoryStream())
            {
                var textWriter = new StreamWriter(stream);
                var writer = new JsonTextWriter(textWriter);

                var mockInnerException = new Mock<Exception>(MockBehavior.Strict);
                mockInnerException.Setup(m => m.Message).Returns("Inner exception message");
                mockInnerException.Setup(m => m.StackTrace).Returns("Inner stack trace");

                var outerException = new Exception("Outer exception message", mockInnerException.Object);

                var error = new HttpError(outerException, true)
                {
                    StackTrace = "Outer stack trace"
                };
                var jsonSerializer = new JsonSerializer();

                var mockIdProvider = new Mock<IErrorIdProvider>(MockBehavior.Strict);
                mockIdProvider.SetupSequence(p => p.GenerateId(It.IsAny<HttpError>())).Returns("OUTER-ID").Returns("INNER-ID");

                var serializer = new ErrorSerializer(mockIdProvider.Object);
                serializer.SerializeError(error, stream, writer, jsonSerializer);

                writer.Flush();

                var expectedJson = File.ReadAllText("ErrorSerializerTest.json");
                var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
                var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
                output.Should().Be(minifiedExpectedJson);
            }
        }
    }
}
