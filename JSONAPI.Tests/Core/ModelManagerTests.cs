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
        public void GetPropertyMapTest()
        {
            // Arrange
            var mm = new ModelManager();

            // Act
            var propMap = mm.GetPropertyMap(typeof(Post));
            
            // Assert
            Assert.AreSame(typeof(Post).GetProperty("Id"), propMap["id"]);
            Assert.AreSame(typeof(Post).GetProperty("Author"), propMap["author"]);
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
    }
}
