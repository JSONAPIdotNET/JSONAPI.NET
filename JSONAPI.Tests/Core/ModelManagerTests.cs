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
            // Act
            PropertyInfo idprop = ModelManager.Instance.GetIdProperty(typeof(Author));

            // Assert
            Assert.AreSame(typeof(Author).GetProperty("Id"), idprop);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void DoesntFindMissingId()
        {
            // Arrange
            // Act
            PropertyInfo idprop = ModelManager.Instance.GetIdProperty(typeof(InvalidModel));

            // Assert
            Assert.Fail("An InvalidOperationException should be thrown and we shouldn't get here!");
        }

        [TestMethod]
        public void GetPropertyMapTest()
        {
            // Arrange
            // Act
            var propMap = ModelManager.Instance.GetPropertyMap(typeof(Post));
            
            // Assert
            Assert.AreSame(typeof(Post).GetProperty("Id"), propMap["id"]);
            Assert.AreSame(typeof(Post).GetProperty("Author"), propMap["author"]);
        }

        [TestMethod]
        public void GetJsonKeyForTypeTest()
        {
            // Arrange
            var pluralizationService = new PluralizationService();

            // Act
            var postKey = ModelManager.Instance.GetJsonKeyForType(typeof(Post), pluralizationService);
            var authorKey = ModelManager.Instance.GetJsonKeyForType(typeof(Author), pluralizationService);
            var commentKey = ModelManager.Instance.GetJsonKeyForType(typeof(Comment), pluralizationService);

            // Assert
            Assert.AreEqual("posts", postKey);
            Assert.AreEqual("authors", authorKey);
            Assert.AreEqual("comments", commentKey);
        }
    }
}
