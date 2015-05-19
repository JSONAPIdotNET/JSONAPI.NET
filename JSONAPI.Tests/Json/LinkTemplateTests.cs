using JSONAPI.Attributes;
using JSONAPI.Json;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Text;
using JSONAPI.Core;

namespace JSONAPI.Tests.Json
{
    [TestClass]
    public class LinkTemplateTests
    {
        private class Post
        {
            public string Id { get; set; }

            public string Title { get; set; }

            [SerializeAs(SerializeAsOptions.Link)]
            [LinkTemplate("/users/{0}")]
            public virtual User Author { get; set; }
        }

        private class User
        {
            public string Id { get; set; }

            public string Name { get; set; }
        }

        private Post ThePost { get; set; }

        [TestInitialize]
        public void SetupModels()
        {
            ThePost = new Post
            {
                Id = "2",
                Title = "How to fry an egg",
                Author = new User
                {
                    Id = "5",
                    Name = "Bob"
                }
            };
        }

        [TestMethod]
        [DeploymentItem(@"Data\LinkTemplateTest.json")]
        public void GetResourceWithLinkTemplateRelationship()
        {
            var modelManager = new ModelManager(new PluralizationService());
            modelManager.RegisterResourceType(typeof(Post));
            modelManager.RegisterResourceType(typeof(User));
            var formatter = new JsonApiFormatter(modelManager);
            var stream = new MemoryStream();

            formatter.WriteToStreamAsync(typeof(Post), ThePost, stream, null, null);

            // Assert
            var expected = JsonHelpers.MinifyJson(File.ReadAllText("LinkTemplateTest.json"));
            var output = Encoding.ASCII.GetString(stream.ToArray());
            Trace.WriteLine(output);
            Assert.AreEqual(output.Trim(), expected);
        }
    }
}
