using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Tests.Models;
using Newtonsoft.Json;
using JSONAPI.Json;
using JSONAPI.Core;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class JsonApiMediaFormaterTests
    {
        Author a;
        Post p, p2, p3, p4;
        Sample s1, s2;

        private class MockErrorSerializer : IErrorSerializer
        {
            public bool CanSerialize(Type type)
            {
                return true;
            }

            public void SerializeError(object error, Stream writeStream, JsonWriter writer, JsonSerializer serializer)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("test");
                serializer.Serialize(writer, "foo");
                writer.WriteEndObject();
            }
        }

        [TestInitialize]
        public void SetupModels()
        {
            a = new Author
            {
                Id = 1,
                Name = "Jason Hater",
            };

            p = new Post()
            {
                Id = 1,
                Title = "Linkbait!",
                Author = a
            };
            p2 = new Post
            {
                Id = 2,
                Title = "Rant #1023",
                Author = a
            };
            p3 = new Post
            {
                Id = 3,
                Title = "Polemic in E-flat minor #824",
                Author = a
            };
            p4 = new Post
            {
                Id = 4,
                Title = "This post has no author."
            };

            a.Posts = new List<Post> { p, p2, p3 };

            p.Comments = new List<Comment>() {
                new Comment() {
                    Id = 2,
                    Body = "Nuh uh!",
                    Post = p
                },
                new Comment() {
                    Id = 3,
                    Body = "Yeah huh!",
                    Post = p
                },
                new Comment() {
                    Id = 4,
                    Body = "Third Reich.",
                    Post = p
                }
            };
            p2.Comments = new List<Comment> {
                new Comment {
                    Id = 5,
                    Body = "I laughed, I cried!",
                    Post = p2
                }
            };

            s1 = new Sample
            {
                Id = "1",
                BooleanField = false,
                NullableBooleanField = false,
                SByteField = default(SByte),
                NullableSByteField = null,
                ByteField = default(Byte),
                NullableByteField = null,
                Int16Field = default(Int16),
                NullableInt16Field = null,
                UInt16Field = default(UInt16),
                NullableUInt16Field = null,
                Int32Field = default(Int32),
                NullableInt32Field = null,
                UInt32Field = default(Int32),
                NullableUInt32Field = null,
                Int64Field = default(Int64),
                NullableInt64Field = null,
                UInt64Field = default(UInt64),
                NullableUInt64Field = null,
                DoubleField = default(Double),
                NullableDoubleField = null,
                SingleField = default(Single),
                NullableSingleField = null,
                DecimalField = default(Decimal),
                NullableDecimalField = null,
                DateTimeField = default(DateTime),
                NullableDateTimeField = null,
                DateTimeOffsetField = default(DateTimeOffset),
                NullableDateTimeOffsetField = null,
                GuidField = default(Guid),
                NullableGuidField = null,
                StringField = default(String),
                EnumField = default(SampleEnum),
                NullableEnumField = null,
            };
            s2 = new Sample
            {
                Id = "2",
                BooleanField = true,
                NullableBooleanField = true,
                SByteField = 123,
                NullableSByteField = 123,
                ByteField = 253,
                NullableByteField = 253,
                Int16Field = 32000,
                NullableInt16Field = 32000,
                UInt16Field = 64000,
                NullableUInt16Field = 64000,
                Int32Field = 2000000000,
                NullableInt32Field = 2000000000,
                UInt32Field = 3000000000,
                NullableUInt32Field = 3000000000,
                Int64Field = 9223372036854775807,
                NullableInt64Field = 9223372036854775807,
                UInt64Field = 9223372036854775808,
                NullableUInt64Field = 9223372036854775808,
                DoubleField = 1056789.123,
                NullableDoubleField = 1056789.123,
                SingleField = 1056789.123f,
                NullableSingleField = 1056789.123f,
                DecimalField = 1056789.123m,
                NullableDecimalField = 1056789.123m,
                DateTimeField = new DateTime(1776, 07, 04),
                NullableDateTimeField = new DateTime(1776, 07, 04),
                DateTimeOffsetField = new DateTimeOffset(new DateTime(1776, 07, 04), new TimeSpan(-5, 0, 0)),
                NullableDateTimeOffsetField = new DateTimeOffset(new DateTime(1776, 07, 04), new TimeSpan(-5, 0, 0)),
                GuidField = new Guid("6566F9B4-5245-40DE-890D-98B40A4AD656"),
                NullableGuidField = new Guid("3D1FB81E-43EE-4D04-AF91-C8A326341293"),
                StringField = "Some string 156",
                EnumField = SampleEnum.Value1,
                NullableEnumField = SampleEnum.Value2,
            };
        }

        [TestMethod]
        [DeploymentItem(@"Data\SerializerIntegrationTest.json")]
        public void SerializerIntegrationTest()
        {
            // Arrange
            //PayloadConverter pc = new PayloadConverter();
            //ModelConverter mc = new ModelConverter();
            //ContractResolver.PluralizationService = new PluralizationService();

            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            // Act
            //Payload payload = new Payload(a.Posts);
            //js.Serialize(jw, payload);
            formatter.WriteToStreamAsync(typeof(Post), new[] { p, p2, p3, p4 }.ToList(), stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            Assert.AreEqual(output.Trim(), File.ReadAllText("SerializerIntegrationTest.json").Trim());
            //Assert.AreEqual("[2,3,4]", sw.ToString());
        }

        [TestMethod]
        [DeploymentItem(@"Data\SerializerIntegrationTest.json")]
        public void SerializeArrayIntegrationTest()
        {
            // Arrange
            //PayloadConverter pc = new PayloadConverter();
            //ModelConverter mc = new ModelConverter();
            //ContractResolver.PluralizationService = new PluralizationService();

            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            // Act
            //Payload payload = new Payload(a.Posts);
            //js.Serialize(jw, payload);
            formatter.WriteToStreamAsync(typeof(Post), new[] { p, p2, p3, p4 }, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            Assert.AreEqual(output.Trim(), File.ReadAllText("SerializerIntegrationTest.json").Trim());
            //Assert.AreEqual("[2,3,4]", sw.ToString());
        }

        [TestMethod]
        [DeploymentItem(@"Data\AttributeSerializationTest.json")]
        public void Serializes_attributes_properly()
        {
            // Arrang
            JsonApiFormatter formatter = new JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(Sample), new[] { s1, s2 }, stream, null, null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            var expected = JsonHelpers.MinifyJson(File.ReadAllText("AttributeSerializationTest.json"));
            Assert.AreEqual(expected, output.Trim());
        }

        [TestMethod]
        [DeploymentItem(@"Data\FormatterErrorSerializationTest.json")]
        public void Should_serialize_error()
        {
            // Arrange
            var formatter = new JSONAPI.Json.JsonApiFormatter(new MockErrorSerializer());
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            var stream = new MemoryStream();

            // Act
            var payload = new HttpError(new Exception(), true);
            formatter.WriteToStreamAsync(typeof(HttpError), payload, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            var expectedJson = File.ReadAllText("FormatterErrorSerializationTest.json");
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        [DeploymentItem(@"Data\ErrorSerializerTest.json")]
        public void SerializeErrorIntegrationTest()
        {
            // Arrange
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            // Act
            var payload = new HttpError(new Exception("This is the exception message!"), true)
            {
                StackTrace = "Stack trace would go here"
            };
            formatter.WriteToStreamAsync(typeof(HttpError), payload, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            var expectedJson = File.ReadAllText("ErrorSerializerTest.json");
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            output = Regex.Replace(output,
                @"[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}",
                "TEST-ERROR-ID"); // We don't know what the GUID will be, so replace it
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        public void DeserializeCollectionIntegrationTest()
        {
            // Arrange
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            formatter.WriteToStreamAsync(typeof(Post), new List<Post> {p, p2}, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);
            stream.Seek(0, SeekOrigin.Begin);

            // Act
            IList<Post> posts;
            posts = (IList<Post>)formatter.ReadFromStreamAsync(typeof(Post), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            // Assert
            Assert.AreEqual(2, posts.Count);
            Assert.AreEqual(p.Id, posts[0].Id); // Order matters, right?

        }

        // Issue #1
        [TestMethod(), Timeout(1000)]
        public void DeserializeExtraPropertyTest()
        {
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""authors"":{""id"":13,""name"":""Jason Hater"",""bogus"":""PANIC!"",""links"":{""posts"":[]}}}"));

            // Act
            Author a;
            a = (Author)formatter.ReadFromStreamAsync(typeof(Author), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            // Assert
            Assert.AreEqual("Jason Hater", a.Name); // Completed without exceptions and didn't timeout!
        }

        // Issue #1
        [TestMethod(), Timeout(1000)]
        public void DeserializeExtraRelationshipTest()
        {
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""authors"":{""id"":13,""name"":""Jason Hater"",""links"":{""posts"":[],""bogus"":[""PANIC!""]}}}"));

            // Act
            Author a;
            a = (Author)formatter.ReadFromStreamAsync(typeof(Author), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            // Assert
            Assert.AreEqual("Jason Hater", a.Name); // Completed without exceptions and didn't timeout!
        }
    }
}
