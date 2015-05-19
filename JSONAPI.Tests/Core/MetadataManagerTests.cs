using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Json;
using System.IO;
using JSONAPI.Tests.Models;
using JSONAPI.Core;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class MetadataManagerTests
    {
        [TestMethod]
        [DeploymentItem(@"Data\MetadataManagerPropertyWasPresentRequest.json")]
        public void PropertyWasPresentTest()
        {
            using (var inputStream = File.OpenRead("MetadataManagerPropertyWasPresentRequest.json"))
            {
                // Arrange
                var modelManager = new ModelManager(new PluralizationService());
                modelManager.RegisterResourceType(typeof(Post));
                modelManager.RegisterResourceType(typeof(Author));
                JsonApiFormatter formatter = new JsonApiFormatter(modelManager);

                var p = (Post) formatter.ReadFromStreamAsync(typeof(Post), inputStream, null, null).Result;

                // Act
                bool idWasSet = MetadataManager.Instance.PropertyWasPresent(p, p.GetType().GetProperty("Id"));
                bool titleWasSet = MetadataManager.Instance.PropertyWasPresent(p, p.GetType().GetProperty("Title"));
                bool authorWasSet = MetadataManager.Instance.PropertyWasPresent(p, p.GetType().GetProperty("Author"));
                bool commentsWasSet = MetadataManager.Instance.PropertyWasPresent(p, p.GetType().GetProperty("Comments"));

                // Assert
                Assert.IsTrue(idWasSet, "Id was not reported as set, but was.");
                Assert.IsFalse(titleWasSet, "Title was reported as set, but was not.");
                Assert.IsTrue(authorWasSet, "Author was not reported as set, but was.");
                Assert.IsFalse(commentsWasSet, "Comments was reported as set, but was not.");
            }
        }
    }
}
