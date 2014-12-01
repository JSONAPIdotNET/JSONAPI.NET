using System;
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
        public void PropertyWasPresentTest()
        {
            // Arrange

            JsonApiFormatter formatter = new JSONAPI.Json.JsonApiFormatter();
            formatter.PluralizationService = new JSONAPI.Core.PluralizationService();
            MemoryStream stream = new MemoryStream();

            stream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(@"{""posts"":{""id"":42,""links"":{""author"":""18""}}}"));

            Post p;
            p = (Post)formatter.ReadFromStreamAsync(typeof(Post), stream, (System.Net.Http.HttpContent)null, (System.Net.Http.Formatting.IFormatterLogger)null).Result;

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
