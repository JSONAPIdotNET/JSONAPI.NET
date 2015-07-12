using System;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Tests.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Core
{
    [TestClass]
    public class ResourceTypeRegistryTests
    {
        private class DerivedPost : Post
        {

        }

        [TestMethod]
        public void GetRegistrationForType_returns_correct_value_for_registered_types()
        {
            // Arrange
            var mockPostRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockPostRegistration.Setup(m => m.Type).Returns(typeof(Post));
            mockPostRegistration.Setup(m => m.ResourceTypeName).Returns("posts");

            var mockAuthorRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockAuthorRegistration.Setup(m => m.Type).Returns(typeof(Author));
            mockAuthorRegistration.Setup(m => m.ResourceTypeName).Returns("authors");

            var registry = new ResourceTypeRegistry();
            registry.AddRegistration(mockPostRegistration.Object);
            registry.AddRegistration(mockAuthorRegistration.Object);

            // Act
            var authorReg = registry.GetRegistrationForType(typeof(Author));
            var postReg = registry.GetRegistrationForType(typeof(Post));

            // Assert
            postReg.Should().BeSameAs(mockPostRegistration.Object);
            authorReg.Should().BeSameAs(mockAuthorRegistration.Object);
        }

        [TestMethod]
        public void GetRegistrationForType_gets_registration_for_closest_registered_base_type_for_unregistered_type()
        {
            // Arrange
            var mockPostRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockPostRegistration.Setup(m => m.Type).Returns(typeof(Post));
            mockPostRegistration.Setup(m => m.ResourceTypeName).Returns("posts");

            var registry = new ResourceTypeRegistry();
            registry.AddRegistration(mockPostRegistration.Object);

            // Act
            var registration = registry.GetRegistrationForType(typeof(DerivedPost));

            // Assert
            registration.Type.Should().Be(typeof(Post));
        }

        [TestMethod]
        public void GetRegistrationForType_fails_when_getting_unregistered_type()
        {
            // Arrange
            var registry = new ResourceTypeRegistry();

            // Act
            Action action = () =>
            {
                registry.GetRegistrationForType(typeof(Post));
            };

            // Assert
            action.ShouldThrow<TypeRegistrationNotFoundException>().WithMessage("No type registration was found for the type \"Post\".");
        }

        [TestMethod]
        public void GetRegistrationForResourceTypeName_fails_when_getting_unregistered_type_name()
        {
            // Arrange
            var registry = new ResourceTypeRegistry();

            // Act
            Action action = () =>
            {
                registry.GetRegistrationForResourceTypeName("posts");
            };

            // Assert
            action.ShouldThrow<TypeRegistrationNotFoundException>().WithMessage("No type registration was found for the type name \"posts\".");
        }

        [TestMethod]
        public void GetRegistrationForResourceTypeName_returns_correct_value_for_registered_names()
        {
            // Arrange
            var mockPostRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockPostRegistration.Setup(m => m.Type).Returns(typeof(Post));
            mockPostRegistration.Setup(m => m.ResourceTypeName).Returns("posts");

            var mockAuthorRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockAuthorRegistration.Setup(m => m.Type).Returns(typeof(Author));
            mockAuthorRegistration.Setup(m => m.ResourceTypeName).Returns("authors");

            var registry = new ResourceTypeRegistry();
            registry.AddRegistration(mockPostRegistration.Object);
            registry.AddRegistration(mockAuthorRegistration.Object);

            // Act
            var postReg = registry.GetRegistrationForResourceTypeName("posts");
            var authorReg = registry.GetRegistrationForResourceTypeName("authors");

            // Assert
            postReg.Should().BeSameAs(mockPostRegistration.Object);
            authorReg.Should().BeSameAs(mockAuthorRegistration.Object);
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_if_type_is_registered()
        {
            // Arrange
            var mockPostRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockPostRegistration.Setup(m => m.Type).Returns(typeof(Post));
            mockPostRegistration.Setup(m => m.ResourceTypeName).Returns("posts");

            var registry = new ResourceTypeRegistry();
            registry.AddRegistration(mockPostRegistration.Object);

            // Act
            var isRegistered = registry.TypeIsRegistered(typeof(Post));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_true_if_parent_type_is_registered()
        {
            // Arrange
            var mockPostRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockPostRegistration.Setup(m => m.Type).Returns(typeof(Post));
            mockPostRegistration.Setup(m => m.ResourceTypeName).Returns("posts");

            var registry = new ResourceTypeRegistry();
            registry.AddRegistration(mockPostRegistration.Object);

            // Act
            var isRegistered = registry.TypeIsRegistered(typeof(DerivedPost));

            // Assert
            isRegistered.Should().BeTrue();
        }

        [TestMethod]
        public void TypeIsRegistered_returns_false_if_no_type_in_hierarchy_is_registered()
        {
            // Arrange
            var registry = new ResourceTypeRegistry();

            // Act
            var isRegistered = registry.TypeIsRegistered(typeof(Comment));

            // Assert
            isRegistered.Should().BeFalse();
        }
    }
}
