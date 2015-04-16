using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
using Moq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class JsonApiMediaFormatterTests
    {
        Author a;
        Post p, p2, p3, p4;
        Sample s1, s2;
        Tag t1, t2, t3;

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

        private class NonStandardIdThing
        {
            [JSONAPI.Attributes.UseAsId]
            public Guid Uuid { get; set; }
            public string Data { get; set; }
        }

        [TestInitialize]
        public void SetupModels()
        {
            a = new Author
            {
                Id = 1,
                Name = "Jason Hater",
            };

            t1 = new Tag 
            {
                Id = 1,
                Text = "Ember"
            };
            t2 = new Tag 
            {
                Id = 2,
                Text = "React"
            };
            t3 = new Tag 
            {
                Id = 3,
                Text = "Angular"
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
                    Post = p,
                    CustomData = "{ \"foo\": \"bar\" }"
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
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Author));
            modelManager.RegisterResourceType(typeof(Comment));
            modelManager.RegisterResourceType(typeof(Post));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(Post), new[] { p, p2, p3, p4 }.ToList(), stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            var expected = JsonHelpers.MinifyJson(File.ReadAllText("SerializerIntegrationTest.json"));
            Assert.AreEqual(expected, output.Trim());
            //Assert.AreEqual("[2,3,4]", sw.ToString());
        }

        [TestMethod]
        [DeploymentItem(@"Data\SerializerIntegrationTest.json")]
        public void SerializeArrayIntegrationTest()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Author));
            modelManager.RegisterResourceType(typeof(Comment));
            modelManager.RegisterResourceType(typeof(Post));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(Post), new[] { p, p2, p3, p4 }, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            var expected = JsonHelpers.MinifyJson(File.ReadAllText("SerializerIntegrationTest.json"));
            Assert.AreEqual(expected, output.Trim());
            //Assert.AreEqual("[2,3,4]", sw.ToString());
        }

        [TestMethod]
        [DeploymentItem(@"Data\AttributeSerializationTest.json")]
        public void Serializes_attributes_properly() 
        {
            // Arrang
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Sample));
            var formatter = new JsonApiFormatter(modelManager);
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
        [DeploymentItem(@"Data\ByteIdSerializationTest.json")]
        public void Serializes_byte_ids_properly() 
        {
            // Arrang
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Tag));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(Tag), new[] { t1, t2, t3 }, stream, null, null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            var expected = JsonHelpers.MinifyJson(File.ReadAllText("ByteIdSerializationTest.json"));
            Assert.AreEqual(expected, output.Trim());
        }

        [TestMethod]
        [DeploymentItem(@"Data\ReformatsRawJsonStringWithUnquotedKeys.json")]
        public void Reformats_raw_json_string_with_unquoted_keys()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Comment));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            var payload = new [] { new Comment { Id = 5, CustomData = "{ unquotedKey: 5 }"}};
            formatter.WriteToStreamAsync(typeof(Comment), payload, stream, null, null);

            // Assert
            var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("ReformatsRawJsonStringWithUnquotedKeys.json"));
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        [DeploymentItem(@"Data\NullResourceResult.json")]
        public void Serializes_null_resource_properly()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Comment));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(Comment), null, stream, null, null);

            // Assert
            var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("NullResourceResult.json"));
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        [DeploymentItem(@"Data\EmptyArrayResult.json")]
        public void Serializes_null_resource_array_as_empty_array()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Comment));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(Comment[]), null, stream, null, null);

            // Assert
            var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("EmptyArrayResult.json"));
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        [DeploymentItem(@"Data\EmptyArrayResult.json")]
        public void Serializes_null_list_as_empty_array()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Comment));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(List<Comment>), null, stream, null, null);

            // Assert
            var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("EmptyArrayResult.json"));
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        [DeploymentItem(@"Data\MalformedRawJsonString.json")]
        public void Does_not_serialize_malformed_raw_json_string()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Comment));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            // Act
            var payload = new[] { new Comment { Id = 5, CustomData = "{ x }" } };
            formatter.WriteToStreamAsync(typeof(Comment), payload, stream, null, null);

            // Assert
            var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("MalformedRawJsonString.json"));
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        [DeploymentItem(@"Data\FormatterErrorSerializationTest.json")]
        public void Should_serialize_error()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            var formatter = new JsonApiFormatter(modelManager, new MockErrorSerializer());
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
            var modelManager = new ModelManager(new PluralizationService());
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            var mockInnerException = new Mock<Exception>(MockBehavior.Strict);
            mockInnerException.Setup(m => m.Message).Returns("Inner exception message");
            mockInnerException.Setup(m => m.StackTrace).Returns("Inner stack trace");

            var outerException = new Exception("Outer exception message", mockInnerException.Object);

            var payload = new HttpError(outerException, true)
            {
                StackTrace = "Outer stack trace"
            };

            // Act
            formatter.WriteToStreamAsync(typeof(HttpError), payload, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            var expectedJson = File.ReadAllText("ErrorSerializerTest.json");
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());

            // We don't know what the GUIDs will be, so replace them
            var regex = new Regex(@"[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}");
            output = regex.Replace(output, "OUTER-ID", 1); 
            output = regex.Replace(output, "INNER-ID", 1);
            output.Should().Be(minifiedExpectedJson);
        }

        [TestMethod]
        [DeploymentItem(@"Data\DeserializeCollectionRequest.json")]
        public void Deserializes_collections_properly()
        {
            using (var inputStream = File.OpenRead("DeserializeCollectionRequest.json"))
            {
                // Arrange
                var modelManager = new ModelManager(new PluralizationService());
                modelManager.RegisterResourceType(typeof(Post));
                modelManager.RegisterResourceType(typeof(Author));
                modelManager.RegisterResourceType(typeof(Comment));
                var formatter = new JsonApiFormatter(modelManager);

                // Act
                var posts = (IList<Post>)formatter.ReadFromStreamAsync(typeof(Post), inputStream, null, null).Result;

                // Assert
                posts.Count.Should().Be(2);
                posts[0].Id.Should().Be(p.Id);
                posts[0].Title.Should().Be(p.Title);
                posts[0].Author.Id.Should().Be(a.Id);
                posts[0].Comments.Count.Should().Be(2);
                posts[0].Comments[0].Id.Should().Be(400);
                posts[0].Comments[1].Id.Should().Be(401);
                posts[1].Id.Should().Be(p2.Id);
                posts[1].Title.Should().Be(p2.Title);
                posts[1].Author.Id.Should().Be(a.Id);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\DeserializeAttributeRequest.json")]
        public async Task Deserializes_attributes_properly()
        {
            using (var inputStream = File.OpenRead("DeserializeAttributeRequest.json"))
            {
                // Arrange
                var modelManager = new ModelManager(new PluralizationService());
                modelManager.RegisterResourceType(typeof(Sample));
                var formatter = new JsonApiFormatter(modelManager);

                // Act
                var deserialized = (IList<Sample>)await formatter.ReadFromStreamAsync(typeof(Sample), inputStream, null, null);

                // Assert
                deserialized.Count.Should().Be(2);
                deserialized[0].ShouldBeEquivalentTo(s1);
                deserialized[1].ShouldBeEquivalentTo(s2);
            }
        }

        [TestMethod]
        [DeploymentItem(@"Data\DeserializeRawJsonTest.json")]
        public async Task DeserializeRawJsonTest()
        {
            using (var inputStream = File.OpenRead("DeserializeRawJsonTest.json"))
            {
                // Arrange
                var modelManager = new ModelManager(new PluralizationService());
                modelManager.RegisterResourceType(typeof(Comment));
                var formatter = new JsonApiFormatter(modelManager);

                // Act
                var comments = ((IEnumerable<Comment>)await formatter.ReadFromStreamAsync(typeof (Comment), inputStream, null, null)).ToArray();

                // Assert
                Assert.AreEqual(2, comments.Count());
                Assert.AreEqual(null, comments[0].CustomData);
                Assert.AreEqual("{\"foo\":\"bar\"}", comments[1].CustomData);
            }
        }

        // Issue #1
        [TestMethod(), Timeout(1000)]
        public void DeserializeExtraPropertyTest()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Author));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""data"":{""id"":13,""name"":""Jason Hater"",""bogus"":""PANIC!"",""links"":{""posts"":{""linkage"":[]}}}}"));

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
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Author));
            var formatter = new JsonApiFormatter(modelManager);
            MemoryStream stream = new MemoryStream();

            stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""data"":{""id"":13,""name"":""Jason Hater"",""links"":{""posts"":{""linkage"":[]},""bogus"":{""linkage"":[]}}}}"));

            // Act
            Author a;
            a = (Author)formatter.ReadFromStreamAsync(typeof(Author), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            // Assert
            Assert.AreEqual("Jason Hater", a.Name); // Completed without exceptions and didn't timeout!
        }

        [TestMethod]
        [DeploymentItem(@"Data\NonStandardIdTest.json")]
        public void SerializeNonStandardIdTest()
        {
            // Arrange
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(NonStandardIdThing));
            var formatter = new JsonApiFormatter(modelManager);
            var stream = new MemoryStream();
            var payload = new List<NonStandardIdThing> {
                new NonStandardIdThing { Uuid = new Guid("0657fd6d-a4ab-43c4-84e5-0933c84b4f4f"), Data = "Swap" }
            };

            // Act
            formatter.WriteToStreamAsync(typeof(List<NonStandardIdThing>), payload, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            var expectedJson = File.ReadAllText("NonStandardIdTest.json");
            var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            output.Should().Be(minifiedExpectedJson);
        }

        #region Non-standard Id attribute tests

        [TestMethod]
        [DeploymentItem(@"Data\NonStandardIdTest.json")]
        public void DeserializeNonStandardIdTest()
        {
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(NonStandardIdThing));
            var formatter = new JsonApiFormatter(modelManager);
            var stream = new FileStream("NonStandardIdTest.json",FileMode.Open);

            // Act
            IList<NonStandardIdThing> things;
            things = (IList<NonStandardIdThing>)formatter.ReadFromStreamAsync(typeof(NonStandardIdThing), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;
            stream.Close();

            // Assert
            things.Count.Should().Be(1);
            things.First().Uuid.Should().Be(new Guid("0657fd6d-a4ab-43c4-84e5-0933c84b4f4f"));
        }

        [TestMethod]
        [DeploymentItem(@"Data\NonStandardIdTest.json")]
        public void DeserializeNonStandardId()
        {
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(NonStandardIdThing));
            var formatter = new JsonApiFormatter(modelManager);
            string json = File.ReadAllText("NonStandardIdTest.json");
            json = Regex.Replace(json, @"""uuid"":\s*""0657fd6d-a4ab-43c4-84e5-0933c84b4f4f""\s*,",""); // remove the uuid attribute
            var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(json));

            // Act
            IList<NonStandardIdThing> things;
            things = (IList<NonStandardIdThing>)formatter.ReadFromStreamAsync(typeof(NonStandardIdThing), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            // Assert
            json.Should().NotContain("uuid", "The \"uuid\" attribute was supposed to be removed, test methodology problem!");
            things.Count.Should().Be(1);
            things.First().Uuid.Should().Be(new Guid("0657fd6d-a4ab-43c4-84e5-0933c84b4f4f"));
        }
    
        #endregion

    }
}
