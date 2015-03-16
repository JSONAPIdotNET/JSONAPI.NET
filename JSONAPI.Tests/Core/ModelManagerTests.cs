using System;
using JSONAPI.Attributes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using System.Reflection;
using System.Collections.Generic;
using System.Collections;
using FluentAssertions;
using Newtonsoft.Json;

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
            [UseAsId]
            public Guid Uuid { get; set; }

            public string Data { get; set; }
        }

        private class DerivedPost : Post
        {
            
        }

        private class Band
        {
            [UseAsId]
            public string BandName { get; set; }

            [JsonProperty("THE-GENRE")]
            public string Genre { get; set; }
        }

        private class Salad
        {
            public string Id { get; set; }

            [JsonProperty("salad-type")]
            public string TheSaladType { get; set; }

            [JsonProperty("salad-type")]
            public string AnotherSaladType { get; set; }
        }

        [TestMethod]
        public void FindsIdNamedId()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());
            mm.RegisterResourceType(typeof(Author));

            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(Author));

            // Assert
            Assert.AreSame(typeof(Author).GetProperty("Id"), idprop);
        }

        [TestMethod]
        public void Cant_register_model_with_missing_id()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());

            // Act
            Action action = () => mm.RegisterResourceType(typeof(InvalidModel));

            // Assert
            action.ShouldThrow<InvalidOperationException>()
                .Which.Message.Should()
                .Be("Unable to determine Id property for type `invalid-models`.");
        }

        [TestMethod]
        public void FindsIdFromAttribute()
        {
            // Arrange
            var mm = new ModelManager(new PluralizationService());
            mm.RegisterResourceType(typeof(CustomIdModel));
            
            // Act
            PropertyInfo idprop = mm.GetIdProperty(typeof(CustomIdModel));

            // Assert
            Assert.AreSame(typeof(CustomIdModel).GetProperty("Uuid"), idprop);
        }

        [TestMethod]
        public void GetResourceTypeName_returns_correct_value_for_registered_types()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            mm.RegisterResourceType(typeof(Post));
            mm.RegisterResourceType(typeof(Author));
            mm.RegisterResourceType(typeof(Comment));
            mm.RegisterResourceType(typeof(UserGroup));

            // Act
            var postKey = mm.GetResourceTypeNameForType(typeof(Post));
            var authorKey = mm.GetResourceTypeNameForType(typeof(Author));
            var commentKey = mm.GetResourceTypeNameForType(typeof(Comment));
            var manyCommentKey = mm.GetResourceTypeNameForType(typeof(Comment[]));
            var userGroupsKey = mm.GetResourceTypeNameForType(typeof(UserGroup));

            // Assert
            Assert.AreEqual("posts", postKey);
            Assert.AreEqual("authors", authorKey);
            Assert.AreEqual("comments", commentKey);
            Assert.AreEqual("comments", manyCommentKey);
            Assert.AreEqual("user-groups", userGroupsKey);
        }

        [TestMethod]
        public void GetResourceTypeNameForType_gets_name_for_closest_registered_base_type_for_unregistered_type()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            mm.RegisterResourceType(typeof(Post));

            // Act
            var resourceTypeName = mm.GetResourceTypeNameForType(typeof(DerivedPost));

            // Assert
            resourceTypeName.Should().Be("posts");
        }

        [TestMethod]
        public void GetResourceTypeNameForType_fails_when_getting_unregistered_type()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);

            // Act
            Action action = () =>
            {
                mm.GetResourceTypeNameForType(typeof(Post));
            };

            // Assert
            action.ShouldThrow<InvalidOperationException>().WithMessage("The type `JSONAPI.Tests.Models.Post` was not registered.");
        }

        [TestMethod]
        public void GetTypeByResourceTypeName_returns_correct_value_for_registered_names()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            mm.RegisterResourceType(typeof(Post));
            mm.RegisterResourceType(typeof(Author));
            mm.RegisterResourceType(typeof(Comment));
            mm.RegisterResourceType(typeof(UserGroup));

            // Act
            var postType = mm.GetTypeByResourceTypeName("posts");
            var authorType = mm.GetTypeByResourceTypeName("authors");
            var commentType = mm.GetTypeByResourceTypeName("comments");
            var userGroupType = mm.GetTypeByResourceTypeName("user-groups");

            // Assert
            postType.Should().Be(typeof (Post));
            authorType.Should().Be(typeof (Author));
            commentType.Should().Be(typeof (Comment));
            userGroupType.Should().Be(typeof (UserGroup));
        }

        [TestMethod]
        public void GetTypeByResourceTypeName_fails_when_getting_unregistered_name()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);

            // Act
            Action action = () =>
            {
                mm.GetTypeByResourceTypeName("posts");
            };

            // Assert
            action.ShouldThrow<InvalidOperationException>().WithMessage("The resource type name `posts` was not registered.");
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_if_type_is_registered()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            mm.RegisterResourceType(typeof (Post));

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof (Post));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_if_parent_type_is_registered()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            mm.RegisterResourceType(typeof(Post));

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(DerivedPost));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_for_collection_of_registered_types()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            mm.RegisterResourceType(typeof(Post));

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(ICollection<Post>));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_for_collection_of_children_of_registered_types()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            mm.RegisterResourceType(typeof(Post));

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(ICollection<DerivedPost>));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_false_if_no_type_in_hierarchy_is_registered()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(Comment));

            // Assert
            isRegistered.Should().BeFalse();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_false_for_collection_of_unregistered_types()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);

            // Act
            var isRegistered = mm.TypeIsRegistered(typeof(ICollection<Comment>));

            // Assert
            isRegistered.Should().BeFalse();
        }

        [TestMethod]
        public void GetJsonKeyForPropertyTest()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);

            // Act
            var idKey = mm.CalculateJsonKeyForProperty(typeof(Author).GetProperty("Id"));
            var nameKey = mm.CalculateJsonKeyForProperty(typeof(Author).GetProperty("Name"));
            var postsKey = mm.CalculateJsonKeyForProperty(typeof(Author).GetProperty("Posts"));

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
            mm.RegisterResourceType(authorType);

            // Act
            var idProp = mm.GetPropertyForJsonKey(authorType, "id");
            var nameProp = mm.GetPropertyForJsonKey(authorType, "name");
            var postsProp = mm.GetPropertyForJsonKey(authorType, "posts");

            // Assert
            idProp.Property.Should().BeSameAs(authorType.GetProperty("Id"));
            idProp.Should().BeOfType<FieldModelProperty>();

            nameProp.Property.Should().BeSameAs(authorType.GetProperty("Name"));
            nameProp.Should().BeOfType<FieldModelProperty>();

            postsProp.Property.Should().BeSameAs(authorType.GetProperty("Posts"));
            postsProp.Should().BeOfType<RelationshipModelProperty>();
        }

        [TestMethod]
        public void GetPropertyForJsonKey_returns_correct_value_for_custom_id()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            Type bandType = typeof(Band);
            mm.RegisterResourceType(bandType);

            // Act
            var idProp = mm.GetPropertyForJsonKey(bandType, "id");

            // Assert
            idProp.Property.Should().BeSameAs(bandType.GetProperty("BandName"));
            idProp.Should().BeOfType<FieldModelProperty>();
        }

        [TestMethod]
        public void GetPropertyForJsonKey_returns_correct_value_for_JsonProperty_attribute()
        {
            // Arrange
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            Type bandType = typeof(Band);
            mm.RegisterResourceType(bandType);

            // Act
            var prop = mm.GetPropertyForJsonKey(bandType, "THE-GENRE");

            // Assert
            prop.Property.Should().BeSameAs(bandType.GetProperty("Genre"));
            prop.Should().BeOfType<FieldModelProperty>();
        }

        [TestMethod]
        public void Cant_register_type_with_two_properties_with_the_same_name()
        {
            var pluralizationService = new PluralizationService();
            var mm = new ModelManager(pluralizationService);
            Type saladType = typeof(Salad);

            // Act
            Action action = () => mm.RegisterResourceType(saladType);

            // Assert
            action.ShouldThrow<InvalidOperationException>().Which.Message.Should().Be("The type `salads` already contains a property keyed at `salad-type`.");
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
