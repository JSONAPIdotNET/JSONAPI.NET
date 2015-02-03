using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class ModelManagerTests
    {
        private class InvalidModel // No Id discernable!
        {
            public string Data { get; set; }
        }

        private class CustomIdModel
        {
            [JSONAPI.Attributes.UseAsId]
            public Guid Uuid { get; set; }

            public string Data { get; set; }
        }

        [TestMethod]
        public void FindsIdNamedId()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());

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
            var mm = new ModelManager(new PluralizationService());

            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(InvalidModel));

            // Assert
            Assert.Fail("An InvalidOperationException should be thrown and we shouldn't get here!");
        }

        [TestMethod]
        public void FindsIdFromAttribute()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());
            
            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(CustomIdModel));
            // Assert
            Assert.AreSame(typeof(CustomIdModel).GetProperty("Uuid"), idprop);
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
            var manyCommentKey = mm.GetJsonKeyForType(typeof(Comment[]));

            // Assert
            Assert.AreEqual("posts", postKey);
            Assert.AreEqual("authors", authorKey);
            Assert.AreEqual("comments", commentKey);
            Assert.AreEqual("comments", manyCommentKey);
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
            Type authorType = typeof(Author);

            // Act
            var idProp = mm.GetPropertyForJsonKey(authorType, "id");
            var nameProp = mm.GetPropertyForJsonKey(authorType, "name");
            var postsProp = mm.GetPropertyForJsonKey(authorType, "posts");

            // Assert
            Assert.AreSame(authorType.GetProperty("Id"), idProp);
            Assert.AreSame(authorType.GetProperty("Name"), nameProp);
            Assert.AreSame(authorType.GetProperty("Posts"), postsProp);

        }

        [TestMethod]
        public void IsSerializedAsManyTest()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());

            // Act
            bool isArray = mm.IsSerializedAsMany(typeof(Post[]));
            bool isGenericEnumerable = mm.IsSerializedAsMany(typeof(IEnumerable<Post>));
            bool isString = mm.IsSerializedAsMany(typeof(string));
            bool isAuthor = mm.IsSerializedAsMany(typeof(Author));
            bool isNonGenericEnumerable = mm.IsSerializedAsMany(typeof(IEnumerable));

            // Assert
            Assert.IsTrue(isArray);
            Assert.IsTrue(isGenericEnumerable);
            Assert.IsFalse(isString);
            Assert.IsFalse(isAuthor);
            Assert.IsFalse(isNonGenericEnumerable);
        }

        [TestMethod]
        public void GetElementTypeTest()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());

            // Act
            Type postTypeFromArray = mm.GetElementType(typeof(Post[]));
            Type postTypeFromEnumerable = mm.GetElementType(typeof(IEnumerable<Post>));

            // Assert
            Assert.AreSame(typeof(Post), postTypeFromArray);
            Assert.AreSame(typeof(Post), postTypeFromEnumerable);
        }

        [TestMethod]
        public void GetElementTypeInvalidArgumentTest()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());

            // Act
            Type x = mm.GetElementType(typeof(Author));

            // Assert
            Assert.IsNull(x, "Return value of GetElementType should be null for a non-Many type argument!");
        }
    }
}
