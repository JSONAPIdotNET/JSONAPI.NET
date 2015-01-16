using System;
using System.IO;
using System.Web.Http;
using FluentAssertions;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class ErrorSerializerTests
    {
        private class TestErrorIdProvider : IErrorIdProvider
        {
            public string GenerateId(HttpError error)
            {
                return "TEST-ERROR-ID";
            }
        }

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
                var error = new HttpError(new Exception("This is the exception message!"), true)
                {
                    StackTrace = "Stack trace would go here"
                };
                var jsonSerializer = new JsonSerializer();

                var serializer = new ErrorSerializer(new TestErrorIdProvider());
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
