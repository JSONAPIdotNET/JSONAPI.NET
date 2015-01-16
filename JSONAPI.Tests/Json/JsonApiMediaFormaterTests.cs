using System;
using System.Linq;
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

        }

        private enum TestEnum
        {
            
        }

        [TestMethod]
        public void CanWritePrimitiveTest()
        {
            // Arrange
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
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
