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

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class JsonApiMediaFormaterTests
    {
        Author a;
        Post p, p2, p3, p4;

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

        }

        private enum TestEnum
        {
            
        }

        [TestMethod]
        public void CanWritePrimitiveTest()
        {
            // Arrange
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new PluralizationService());
            // Act
            // Assert
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(Int32)), "CanWriteTypeAsPrimitive returned wrong answer for Integer!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(Double)), "CanWriteTypeAsPrimitive returned wrong answer for Double!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(DateTime)), "CanWriteTypeAsPrimitive returned wrong answer for DateTime!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(DateTimeOffset)), "CanWriteTypeAsPrimitive returned wrong answer for DateTimeOffset!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(Guid)), "CanWriteTypeAsPrimitive returned wrong answer for Guid!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(String)), "CanWriteTypeAsPrimitive returned wrong answer for String!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(DateTime?)), "CanWriteTypeAsPrimitive returned wrong answer for nullable DateTime!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(DateTimeOffset?)), "CanWriteTypeAsPrimitive returned wrong answer for nullable DateTimeOffset!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(Guid?)), "CanWriteTypeAsPrimitive returned wrong answer for nullable Guid!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(TestEnum)), "CanWriteTypeAsPrimitive returned wrong answer for enum!");
            Assert.IsTrue(formatter.CanWriteTypeAsPrimitive(typeof(TestEnum?)), "CanWriteTypeAsPrimitive returned wrong answer for nullable enum!");
            Assert.IsFalse(formatter.CanWriteTypeAsPrimitive(typeof(Object)), "CanWriteTypeAsPrimitive returned wrong answer for Object!");
        }

        [TestMethod]
        [DeploymentItem(@"Data\SerializerIntegrationTest.json")]
        public void SerializerIntegrationTest()
        {
            // Arrange
            //PayloadConverter pc = new PayloadConverter();
            //ModelConverter mc = new ModelConverter();
            //ContractResolver.PluralizationService = new PluralizationService();

            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
            MemoryStream stream = new MemoryStream();

            // Act
            //Payload payload = new Payload(a.Posts);
            //js.Serialize(jw, payload);
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
            //PayloadConverter pc = new PayloadConverter();
            //ModelConverter mc = new ModelConverter();
            //ContractResolver.PluralizationService = new PluralizationService();

            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
            MemoryStream stream = new MemoryStream();

            // Act
            //Payload payload = new Payload(a.Posts);
            //js.Serialize(jw, payload);
            formatter.WriteToStreamAsync(typeof(Post), new[] { p, p2, p3, p4 }, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            var expected = JsonHelpers.MinifyJson(File.ReadAllText("SerializerIntegrationTest.json"));
            Assert.AreEqual(expected, output.Trim());
            //Assert.AreEqual("[2,3,4]", sw.ToString());
        }

        [TestMethod]
        [DeploymentItem(@"Data\FormatterErrorSerializationTest.json")]
        public void Should_serialize_error()
        {
            // Arrange
            var formatter = new JSONAPI.Json.JsonApiFormatter(new MockErrorSerializer());
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
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
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
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
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

        [TestMethod]
        [DeploymentItem(@"Data\DeserializeRawJsonTest.json")]
        public async Task DeserializeRawJsonTest()
        {
            using (var inputStream = File.OpenRead("DeserializeRawJsonTest.json"))
            {
                // Arrange
                var formatter = new JsonApiFormatter(new PluralizationService());

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
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
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
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
            MemoryStream stream = new MemoryStream();

            stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""authors"":{""id"":13,""name"":""Jason Hater"",""links"":{""posts"":[],""bogus"":[""PANIC!""]}}}"));

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
            var formatter = new JSONAPI.Json.JsonApiFormatter(new PluralizationService());
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
            var formatter = new JSONAPI.Json.JsonApiFormatter(new PluralizationService());
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
        public void DeserializeNonStandardIdWithIdOnly()
        {
            var formatter = new JSONAPI.Json.JsonApiFormatter(new PluralizationService());
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

        [TestMethod]
        [DeploymentItem(@"Data\NonStandardIdTest.json")]
        public void DeserializeNonStandardIdWithoutId()
        {
            var formatter = new JSONAPI.Json.JsonApiFormatter(new PluralizationService());
            string json = File.ReadAllText("NonStandardIdTest.json");
            json = Regex.Replace(json, @"""id"":\s*""0657fd6d-a4ab-43c4-84e5-0933c84b4f4f""\s*,", ""); // remove the uuid attribute
            var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(json));

            // Act
            IList<NonStandardIdThing> things;
            things = (IList<NonStandardIdThing>)formatter.ReadFromStreamAsync(typeof(NonStandardIdThing), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            // Assert
            json.Should().NotContain("\"id\"", "The \"id\" attribute was supposed to be removed, test methodology problem!");
            things.Count.Should().Be(1);
            things.First().Uuid.Should().Be(new Guid("0657fd6d-a4ab-43c4-84e5-0933c84b4f4f"));

        }
    
        #endregion

    }
}
