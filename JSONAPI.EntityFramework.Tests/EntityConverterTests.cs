using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using JSONAPI.Core;
using JSONAPI.Json;
using JSONAPI.EntityFramework;
using JSONAPI.EntityFramework.Tests.Models;
using System.IO;
using System.Diagnostics;
using System.Reflection;
using System.Data;
using System.Text.RegularExpressions;

namespace JSONAPI.EntityFramework.Tests
{
    [TestClass]
    public class EntityConverterTests
    {
        private TestEntities context;
        private Author a, a2;
        private Post p, p2, p3;
        private Comment c, c2, c3, c4;

        [TestInitialize]
        public void SetupEntities()
        {
            //- See http://stackoverflow.com/a/19130718/489116
            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
            //-

            context = new TestEntities();
            //JSONAPI.EntityFramework.Json.ContractResolver.ObjectContext = context;


            // Clear it out!
            foreach (Comment o in context.Comments) context.Comments.Remove(o);
            foreach (Post o in context.Posts) context.Posts.Remove(o);
            foreach (Author o in context.Authors) context.Authors.Remove(o);
            context.SaveChanges();


            a = new Author
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Jason Hater"
            };
            context.Authors.Add(a);

            p = new Post()
            {
                Title = "Linkbait!",
                Author = a
            };
            p2 = new Post
            {
                Title = "Rant #1023",
                Author = a
            };
            p3 = new Post
            {
                Title = "Polemic in E-flat minor #824",
                Author = a
            };

            a.Posts.Add(p);
            a.Posts.Add(p2);
            a.Posts.Add(p3);

            p.Comments.Add(
                c = new Comment()
                {
                    Body = "Nuh uh!",
                    Post = p
                }
            );
            p.Comments.Add(
                c2 = new Comment()
                {
                    Body = "Yeah huh!",
                    Post = p
                }
            );
            p.Comments.Add(
                c3 = new Comment()
                {
                    Body = "Third Reich.",
                    Post = p
                }
            );

            p2.Comments.Add(
                c4 = new Comment
                {
                    Body = "I laughed, I cried!",
                    Post = p2
                }
            );

            context.SaveChanges();
        }
        
        [TestMethod]
        public void SerializeTest()
        {
            // Arrange
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
            MemoryStream stream = new MemoryStream();

            // Act
            formatter.WriteToStreamAsync(typeof(Post), p.Comments.First(), stream, (System.Net.Http.HttpContent)null, (System.Net.TransportContext)null);

            // Assert
            string output = System.Text.Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);

        }

        [TestMethod]
        public async Task UnderpostingTest()
        {
            // Arrange
            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter(new JSONAPI.Core.PluralizationService());
            MemoryStream stream = new MemoryStream();

            EntityFrameworkMaterializer materializer = new EntityFrameworkMaterializer(context, MetadataManager.Instance);

            string underpost = @"{""posts"":{""id"":""" + p.Id.ToString() + @""",""title"":""Not at all linkbait!""}}";
            stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(underpost));

            int previousCommentsCount = p.Comments.Count;

            // Act
            Post pUpdated;
            pUpdated = (Post)await formatter.ReadFromStreamAsync(typeof(Post), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null);
            pUpdated = await materializer.MaterializeUpdateAsync<Post>(pUpdated);

            // Assert
            Assert.AreEqual(previousCommentsCount, pUpdated.Comments.Count, "Comments were wiped out!");
            Assert.AreEqual("Not at all linkbait!", pUpdated.Title, "Title was not updated.");
        }

    }
}
