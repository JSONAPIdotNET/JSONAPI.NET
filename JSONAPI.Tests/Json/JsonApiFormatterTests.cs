using System;
using System.Threading.Tasks;
using System.Web.Http;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using JSONAPI.Json;
using System.IO;
using System.Net;
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Moq;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class JsonApiFormatterTests
    {
        private JsonApiFormatter BuildFormatter(ISingleResourcePayloadSerializer singleResourcePayloadSerializer = null,
            IResourceCollectionPayloadSerializer resourceCollectionPayloadSerializer = null,
            IErrorPayloadSerializer errorPayloadSerializer = null,
            IErrorPayloadBuilder errorPayloadBuilder = null)
        {

            singleResourcePayloadSerializer = singleResourcePayloadSerializer ?? new Mock<ISingleResourcePayloadSerializer>(MockBehavior.Strict).Object;
            resourceCollectionPayloadSerializer = resourceCollectionPayloadSerializer ?? new Mock<IResourceCollectionPayloadSerializer>(MockBehavior.Strict).Object;
            errorPayloadSerializer = errorPayloadSerializer ?? new Mock<IErrorPayloadSerializer>(MockBehavior.Strict).Object;
            errorPayloadBuilder = errorPayloadBuilder ?? new ErrorPayloadBuilder();
            return new JsonApiFormatter(singleResourcePayloadSerializer, resourceCollectionPayloadSerializer, errorPayloadSerializer, errorPayloadBuilder);
        }

        [TestMethod]
        public void Serialize_SingleResourcePayload()
        {
            // Arrange
            var payload = new Mock<ISingleResourcePayload>(MockBehavior.Strict);
            var singleResourcePayloadSerializer = new Mock<ISingleResourcePayloadSerializer>(MockBehavior.Strict);
            singleResourcePayloadSerializer.Setup(s => s.Serialize(payload.Object, It.IsAny<JsonWriter>()))
                .Returns((ISingleResourcePayload p, JsonWriter writer) =>
                {
                    writer.WriteValue("SingleResourcePayload output goes here.");
                    return Task.FromResult(0);
                });

            var formatter = BuildFormatter(singleResourcePayloadSerializer.Object);
            var stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(payload.Object.GetType(), payload.Object, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_SingleResourcePayload.json");
        }

        [TestMethod]
        public void Serialize_ResourceCollectionPayload()
        {
            // Arrange
            var payload = new Mock<IResourceCollectionPayload>(MockBehavior.Strict);
            var resourceCollectionPayloadSerializer = new Mock<IResourceCollectionPayloadSerializer>(MockBehavior.Strict);
            resourceCollectionPayloadSerializer.Setup(s => s.Serialize(payload.Object, It.IsAny<JsonWriter>()))
                .Returns((IResourceCollectionPayload p, JsonWriter writer) =>
                {
                    writer.WriteValue("ResourceCollectionPayload output goes here.");
                    return Task.FromResult(0);
                });

            var formatter = BuildFormatter(resourceCollectionPayloadSerializer: resourceCollectionPayloadSerializer.Object);
            var stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(payload.Object.GetType(), payload.Object, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_ResourceCollectionPayload.json");
        }

        [TestMethod]
        public void Serialize_ErrorPayload()
        {
            // Arrange
            var payload = new Mock<IErrorPayload>(MockBehavior.Strict);
            var errorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);
            errorPayloadSerializer.Setup(s => s.Serialize(payload.Object, It.IsAny<JsonWriter>()))
                .Returns((IErrorPayload p, JsonWriter writer) =>
                {
                    writer.WriteValue("ErrorPayload output goes here.");
                    return Task.FromResult(0);
                });

            var formatter = BuildFormatter(errorPayloadSerializer: errorPayloadSerializer.Object);
            var stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(payload.Object.GetType(), payload.Object, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_ErrorPayload.json");
        }

        [TestMethod]
        public void Serialize_HttpError()
        {
            // Arrange
            var payload = new HttpError(new Exception("This is the exception message"), true);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayload = new Mock<IErrorPayload>(MockBehavior.Strict);
            mockErrorPayloadBuilder.Setup(b => b.BuildFromHttpError(payload, HttpStatusCode.InternalServerError))
                .Returns(mockErrorPayload.Object);

            var mockErrorPayloadSerializer = new Mock<IErrorPayloadSerializer>(MockBehavior.Strict);
            mockErrorPayloadSerializer.Setup(s => s.Serialize(mockErrorPayload.Object, It.IsAny<JsonWriter>()))
                .Returns((IErrorPayload errorPayload, JsonWriter writer) =>
                {
                    writer.WriteValue("HttpError payload");
                    return Task.FromResult(0);
                });

            var stream = new MemoryStream();

            // Act
            var formatter = BuildFormatter(errorPayloadBuilder: mockErrorPayloadBuilder.Object, errorPayloadSerializer: mockErrorPayloadSerializer.Object);
            formatter.WriteToStreamAsync(payload.GetType(), payload, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Serialize_HttpError.json");
        }

        private class Color
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        [TestMethod]
        public void Writes_error_for_anything_else()
        {
            // Arrange
            var formatter = BuildFormatter();
            var stream = new MemoryStream();

            // Act
            var payload = new Color { Id = "1", Name = "Blue" };
            formatter.WriteToStreamAsync(payload.GetType(), payload, stream, null, null).Wait();

            // Assert
            TestHelpers.StreamContentsMatchFixtureContents(stream, "Json/Fixtures/JsonApiFormatter/Writes_error_for_anything_else.json");
        }

        [TestMethod]
        public void ReadFromStreamAsync_deserializes_ISingleResourcePayload()
        {
            // Arrange
            var mockSingleResourcePayload = new Mock<ISingleResourcePayload>(MockBehavior.Strict);
            var singleResourcePayloadSerializer = new Mock<ISingleResourcePayloadSerializer>(MockBehavior.Strict);
            singleResourcePayloadSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), ""))
                .Returns(Task.FromResult(mockSingleResourcePayload.Object));

            var formatter = BuildFormatter(singleResourcePayloadSerializer.Object);
            var stream = new MemoryStream();

            // Act
            var deserialized = formatter.ReadFromStreamAsync(typeof(ISingleResourcePayload), stream, null, null).Result;

            // Assert
            deserialized.Should().BeSameAs(mockSingleResourcePayload.Object);
        }

        [TestMethod]
        public void ReadFromStreamAsync_deserializes_IResourceCollectionPayload()
        {
            // Arrange
            var mockResourceCollectionPayload = new Mock<IResourceCollectionPayload>(MockBehavior.Strict);
            var resourceCollectionPayloadSerializer = new Mock<IResourceCollectionPayloadSerializer>(MockBehavior.Strict);
            resourceCollectionPayloadSerializer.Setup(s => s.Deserialize(It.IsAny<JsonReader>(), ""))
                .Returns(Task.FromResult(mockResourceCollectionPayload.Object));

            var formatter = BuildFormatter(resourceCollectionPayloadSerializer: resourceCollectionPayloadSerializer.Object);
            var stream = new MemoryStream();

            // Act
            var deserialized = formatter.ReadFromStreamAsync(typeof(IResourceCollectionPayload), stream, null, null).Result;

            // Assert
            deserialized.Should().BeSameAs(mockResourceCollectionPayload.Object);
        }

        //Author a;
        //Post p, p2, p3, p4;
        //Sample s1, s2;
        //Tag t1, t2, t3;

        //private class MockErrorSerializer : IErrorSerializer
        //{
        //    public bool CanSerialize(Type type)
        //    {
        //        return true;
        //    }

        //    public void SerializeError(object error, Stream writeStream, JsonWriter writer, JsonSerializer serializer)
        //    {
        //        writer.WriteStartObject();
        //        writer.WritePropertyName("test");
        //        serializer.Serialize(writer, "foo");
        //        writer.WriteEndObject();
        //    }
        //}

        //private class NonStandardIdThing
        //{
        //    [JSONAPI.Attributes.UseAsId]
        //    public Guid Uuid { get; set; }
        //    public string Data { get; set; }
        //}

        //[TestInitialize]
        //public void SetupModels()
        //{
        //    a = new Author
        //    {
        //        Id = 1,
        //        Name = "Jason Hater",
        //    };

        //    t1 = new Tag 
        //    {
        //        Id = 1,
        //        Text = "Ember"
        //    };
        //    t2 = new Tag 
        //    {
        //        Id = 2,
        //        Text = "React"
        //    };
        //    t3 = new Tag 
        //    {
        //        Id = 3,
        //        Text = "Angular"
        //    };

        //    p = new Post()
        //    {
        //        Id = 1,
        //        Title = "Linkbait!",
        //        Author = a
        //    };
        //    p2 = new Post
        //    {
        //        Id = 2,
        //        Title = "Rant #1023",
        //        Author = a
        //    };
        //    p3 = new Post
        //    {
        //        Id = 3,
        //        Title = "Polemic in E-flat minor #824",
        //        Author = a
        //    };
        //    p4 = new Post
        //    {
        //        Id = 4,
        //        Title = "This post has no author."
        //    };

        //    a.Posts = new List<Post> { p, p2, p3 };

        //    p.Comments = new List<Comment>() {
        //        new Comment() {
        //            Id = 2,
        //            Body = "Nuh uh!",
        //            Post = p
        //        },
        //        new Comment() {
        //            Id = 3,
        //            Body = "Yeah huh!",
        //            Post = p
        //        },
        //        new Comment() {
        //            Id = 4,
        //            Body = "Third Reich.",
        //            Post = p,
        //            CustomData = "{ \"foo\": \"bar\" }"
        //        }
        //    };
        //    p2.Comments = new List<Comment> {
        //        new Comment {
        //            Id = 5,
        //            Body = "I laughed, I cried!",
        //            Post = p2
        //        }
        //    };

        //}

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\SerializerIntegrationTest.json")]
        public void SerializerIntegrationTest()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Author));
            //modelManager.RegisterResourceType(typeof(Comment));
            //modelManager.RegisterResourceType(typeof(Post));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //formatter.WriteToStreamAsync(typeof(Post), new[] { p, p2, p3, p4 }.ToList(), stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            //// Assert
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //var expected = JsonHelpers.MinifyJson(File.ReadAllText("SerializerIntegrationTest.json"));
            //Assert.AreEqual(expected, output.Trim());
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\SerializerIntegrationTest.json")]
        public void SerializeArrayIntegrationTest()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Author));
            //modelManager.RegisterResourceType(typeof(Comment));
            //modelManager.RegisterResourceType(typeof(Post));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //formatter.WriteToStreamAsync(typeof(Post), new[] { p, p2, p3, p4 }, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            //// Assert
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //var expected = JsonHelpers.MinifyJson(File.ReadAllText("SerializerIntegrationTest.json"));
            //Assert.AreEqual(expected, output.Trim());
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\AttributeSerializationTest.json")]
        public void Serializes_attributes_properly() 
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Sample));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //formatter.WriteToStreamAsync(typeof(Sample), new[] { s1, s2 }, stream, null, null);

            //// Assert
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //var expected = JsonHelpers.MinifyJson(File.ReadAllText("AttributeSerializationTest.json"));
            //Assert.AreEqual(expected, output.Trim());
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\ByteIdSerializationTest.json")]
        public void Serializes_byte_ids_properly() 
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Tag));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //formatter.WriteToStreamAsync(typeof(Tag), new[] { t1, t2, t3 }, stream, null, null);

            //// Assert
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //var expected = JsonHelpers.MinifyJson(File.ReadAllText("ByteIdSerializationTest.json"));
            //Assert.AreEqual(expected, output.Trim());
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\ReformatsRawJsonStringWithUnquotedKeys.json")]
        public void Reformats_raw_json_string_with_unquoted_keys()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Comment));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //var payload = new [] { new Comment { Id = 5, CustomData = "{ unquotedKey: 5 }"}};
            //formatter.WriteToStreamAsync(typeof(Comment), payload, stream, null, null);

            //// Assert
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("ReformatsRawJsonStringWithUnquotedKeys.json"));
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //output.Should().Be(minifiedExpectedJson);
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\NullResourceResult.json")]
        public void Serializes_null_resource_properly()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Comment));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //formatter.WriteToStreamAsync(typeof(Comment), null, stream, null, null);

            //// Assert
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("NullResourceResult.json"));
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //output.Should().Be(minifiedExpectedJson);
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\EmptyArrayResult.json")]
        public void Serializes_null_resource_array_as_empty_array()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Comment));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //formatter.WriteToStreamAsync(typeof(Comment[]), null, stream, null, null);

            //// Assert
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("EmptyArrayResult.json"));
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //output.Should().Be(minifiedExpectedJson);
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\EmptyArrayResult.json")]
        public void Serializes_null_list_as_empty_array()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Comment));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //formatter.WriteToStreamAsync(typeof(List<Comment>), null, stream, null, null);

            //// Assert
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("EmptyArrayResult.json"));
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //output.Should().Be(minifiedExpectedJson);
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\MalformedRawJsonString.json")]
        public void Does_not_serialize_malformed_raw_json_string()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Comment));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //// Act
            //var payload = new[] { new Comment { Id = 5, CustomData = "{ x }" } };
            //formatter.WriteToStreamAsync(typeof(Comment), payload, stream, null, null);

            //// Assert
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(File.ReadAllText("MalformedRawJsonString.json"));
            //string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //Trace.WriteLine(output);
            //output.Should().Be(minifiedExpectedJson);
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\FormatterErrorSerializationTest.json")]
        public void Should_serialize_error()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //var formatter = new JsonApiFormatter(modelManager, new MockErrorSerializer());
            //var stream = new MemoryStream();

            //// Act
            //var payload = new HttpError(new Exception(), true);
            //formatter.WriteToStreamAsync(typeof(HttpError), payload, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            //// Assert
            //var expectedJson = File.ReadAllText("FormatterErrorSerializationTest.json");
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            //var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //output.Should().Be(minifiedExpectedJson);
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\ErrorSerializerTest.json")]
        public void SerializeErrorIntegrationTest()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //var mockInnerException = new Mock<Exception>(MockBehavior.Strict);
            //mockInnerException.Setup(m => m.Message).Returns("Inner exception message");
            //mockInnerException.Setup(m => m.StackTrace).Returns("Inner stack trace");

            //var outerException = new Exception("Outer exception message", mockInnerException.Object);

            //var payload = new HttpError(outerException, true)
            //{
            //    StackTrace = "Outer stack trace"
            //};

            //// Act
            //formatter.WriteToStreamAsync(typeof(HttpError), payload, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            //// Assert
            //var expectedJson = File.ReadAllText("ErrorSerializerTest.json");
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            //var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());

            //// We don't know what the GUIDs will be, so replace them
            //var regex = new Regex(@"[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}");
            //output = regex.Replace(output, "OUTER-ID", 1); 
            //output = regex.Replace(output, "INNER-ID", 1);
            //output.Should().Be(minifiedExpectedJson);
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\DeserializeCollectionRequest.json")]
        public void Deserializes_collections_properly()
        {
            //using (var inputStream = File.OpenRead("DeserializeCollectionRequest.json"))
            //{
            //    // Arrange
            //    var modelManager = new ModelManager(new PluralizationService());
            //    modelManager.RegisterResourceType(typeof(Post));
            //    modelManager.RegisterResourceType(typeof(Author));
            //    modelManager.RegisterResourceType(typeof(Comment));
            //    var formatter = new JsonApiFormatter(modelManager);

            //    // Act
            //    var posts = (IList<Post>)formatter.ReadFromStreamAsync(typeof(Post), inputStream, null, null).Result;

            //    // Assert
            //    posts.Count.Should().Be(2);
            //    posts[0].Id.Should().Be(p.Id);
            //    posts[0].Title.Should().Be(p.Title);
            //    posts[0].Author.Id.Should().Be(a.Id);
            //    posts[0].Comments.Count.Should().Be(2);
            //    posts[0].Comments[0].Id.Should().Be(400);
            //    posts[0].Comments[1].Id.Should().Be(401);
            //    posts[1].Id.Should().Be(p2.Id);
            //    posts[1].Title.Should().Be(p2.Title);
            //    posts[1].Author.Id.Should().Be(a.Id);
            //}
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\DeserializeAttributeRequest.json")]
        public async Task Deserializes_attributes_properly()
        {
            //using (var inputStream = File.OpenRead("DeserializeAttributeRequest.json"))
            //{
            //    // Arrange
            //    var modelManager = new ModelManager(new PluralizationService());
            //    modelManager.RegisterResourceType(typeof(Sample));
            //    var formatter = new JsonApiFormatter(modelManager);

            //    // Act
            //    var deserialized = (IList<Sample>)await formatter.ReadFromStreamAsync(typeof(Sample), inputStream, null, null);

            //    // Assert
            //    deserialized.Count.Should().Be(2);
            //    deserialized[0].ShouldBeEquivalentTo(s1);
            //    deserialized[1].ShouldBeEquivalentTo(s2);
            //}
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\DeserializeRawJsonTest.json")]
        public async Task DeserializeRawJsonTest()
        {
            //using (var inputStream = File.OpenRead("DeserializeRawJsonTest.json"))
            //{
            //    // Arrange
            //    var modelManager = new ModelManager(new PluralizationService());
            //    modelManager.RegisterResourceType(typeof(Comment));
            //    var formatter = new JsonApiFormatter(modelManager);

            //    // Act
            //    var comments = ((IEnumerable<Comment>)await formatter.ReadFromStreamAsync(typeof (Comment), inputStream, null, null)).ToArray();

            //    // Assert
            //    Assert.AreEqual(2, comments.Count());
            //    Assert.AreEqual(null, comments[0].CustomData);
            //    Assert.AreEqual("{\"foo\":\"bar\"}", comments[1].CustomData);
            //}
        }

        // Issue #1
        [Ignore]
        [TestMethod(), Timeout(1000)]
        public void DeserializeExtraPropertyTest()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Author));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""data"":{""id"":13,""type"":""authors"",""attributes"":{""name"":""Jason Hater"",""bogus"":""PANIC!""},""relationships"":{""posts"":{""data"":[]}}}}"));

            //// Act
            //Author a;
            //a = (Author)formatter.ReadFromStreamAsync(typeof(Author), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            //// Assert
            //Assert.AreEqual("Jason Hater", a.Name); // Completed without exceptions and didn't timeout!
        }

        // Issue #1
        [Ignore]
        [TestMethod(), Timeout(1000)]
        public void DeserializeExtraRelationshipTest()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(Author));
            //var formatter = new JsonApiFormatter(modelManager);
            //MemoryStream stream = new MemoryStream();

            //stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""data"":{""id"":13,""type"":""authors"",""attributes"":{""name"":""Jason Hater""},""relationships"":{""posts"":{""data"":[]},""bogus"":{""data"":[]}}}}"));

            //// Act
            //Author a;
            //a = (Author)formatter.ReadFromStreamAsync(typeof(Author), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            //// Assert
            //Assert.AreEqual("Jason Hater", a.Name); // Completed without exceptions and didn't timeout!
        }

        [TestMethod]
        [Ignore]
        [DeploymentItem(@"Data\NonStandardIdTest.json")]
        public void SerializeNonStandardIdTest()
        {
            //// Arrange
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(NonStandardIdThing));
            //var formatter = new JsonApiFormatter(modelManager);
            //var stream = new MemoryStream();
            //var payload = new List<NonStandardIdThing> {
            //    new NonStandardIdThing { Uuid = new Guid("0657fd6d-a4ab-43c4-84e5-0933c84b4f4f"), Data = "Swap" }
            //};

            //// Act
            //formatter.WriteToStreamAsync(typeof(List<NonStandardIdThing>), payload, stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            //// Assert
            //var expectedJson = File.ReadAllText("NonStandardIdTest.json");
            //var minifiedExpectedJson = JsonHelpers.MinifyJson(expectedJson);
            //var output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            //output.Should().Be(minifiedExpectedJson);
        }

        #region Non-standard Id attribute tests

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\NonStandardIdTest.json")]
        public void DeserializeNonStandardIdTest()
        {
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(NonStandardIdThing));
            //var formatter = new JsonApiFormatter(modelManager);
            //var stream = new FileStream("NonStandardIdTest.json",FileMode.Open);

            //// Act
            //IList<NonStandardIdThing> things;
            //things = (IList<NonStandardIdThing>)formatter.ReadFromStreamAsync(typeof(NonStandardIdThing), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;
            //stream.Close();

            //// Assert
            //things.Count.Should().Be(1);
            //things.First().Uuid.Should().Be(new Guid("0657fd6d-a4ab-43c4-84e5-0933c84b4f4f"));
        }

        [Ignore]
        [TestMethod]
        [DeploymentItem(@"Data\NonStandardIdTest.json")]
        public void DeserializeNonStandardId()
        {
            //var modelManager = new ModelManager(new PluralizationService());
            //modelManager.RegisterResourceType(typeof(NonStandardIdThing));
            //var formatter = new JsonApiFormatter(modelManager);
            //string json = File.ReadAllText("NonStandardIdTest.json");
            //json = Regex.Replace(json, @"""uuid"":\s*""0657fd6d-a4ab-43c4-84e5-0933c84b4f4f""\s*,",""); // remove the uuid attribute
            //var stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(json));

            //// Act
            //IList<NonStandardIdThing> things;
            //things = (IList<NonStandardIdThing>)formatter.ReadFromStreamAsync(typeof(NonStandardIdThing), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

            //// Assert
            //json.Should().NotContain("uuid", "The \"uuid\" attribute was supposed to be removed, test methodology problem!");
            //things.Count.Should().Be(1);
            //things.First().Uuid.Should().Be(new Guid("0657fd6d-a4ab-43c4-84e5-0933c84b4f4f"));
        }
    
        #endregion

    }
}
