using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using System.Reflection;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class ModelManagerTests
    {
        private class InvalidModel
        {
            public string Data { get; set; }
        }

        [TestMethod]
        public void FindsIdNamedId()
        {
            // Arrange
            var mm = new ModelManager();

            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(Author));

            // Assert
            Assert.AreSame(typeof(Author).GetProperty("Id"), idprop);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DoesntFindMissingId()
        {
            // Arrange
            var mm = new ModelManager();

            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(InvalidModel));

            // Assert
            Assert.Fail("An InvalidOperationException should be thrown and we shouldn't get here!");
        }

        [TestMethod]
        public void GetJsonKeyForTypeTest()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);

            // Act
            var postKey = mm.GetJsonKeyForType(typeof(Post));
            var authorKey = mm.GetJsonKeyForType(typeof(Author));
            var commentKey = mm.GetJsonKeyForType(typeof(Comment));

            // Assert
            Assert.AreEqual("posts", postKey);
            Assert.AreEqual("authors", authorKey);
            Assert.AreEqual("comments", commentKey);
        }

        [TestMethod]
        public void GetJsonKeyForPropertyTest()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);

            // Act
            var idKey = mm.GetJsonKeyForProperty(typeof(Author).GetProperty("Id"));
            var nameKey = mm.GetJsonKeyForProperty(typeof(Author).GetProperty("Name"));
            var postsKey = mm.GetJsonKeyForProperty(typeof(Author).GetProperty("Posts"));

            // Assert
            Assert.AreEqual("id", idKey);
            Assert.AreEqual("name", nameKey);
            Assert.AreEqual("posts", postsKey);

        }
        
        [TestMethod]
        public void GetPropertyForJsonKeyTest()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            Type authorType = typeof(Author).GetType();

            // Act
            var idProp = mm.GetPropertyForJsonKey(authorType, "id");
            var nameProp = mm.GetPropertyForJsonKey(authorType, "name");
            var postsProp = mm.GetPropertyForJsonKey(authorType, "posts");

            // Assert
            Assert.AreSame(authorType.GetProperty("Id"), idProp);
            Assert.AreSame(authorType.GetProperty("Name"), nameProp);
            Assert.AreSame(authorType.GetProperty("Posts"), postsProp);

        }
    }
}
