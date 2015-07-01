using System.Collections.Generic;
using FluentAssertions;
using JSONAPI.Core;
using JSONAPI.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Documents
{
    [TestClass]
    public class DefaultLinkConventionsTests
    {
        class Country
        {
            public string Id { get; set; }

            public ICollection<City> Cities { get; set; }
        }

        class City
        {
            public string Id { get; set; }
        }

        [TestMethod]
        public void GetRelationshipLink_returns_default_url_for_relationship()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof (Country).GetProperty("Cities"),
                "cities", typeof (City), null, null);
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.ResourceTypeName).Returns("countries");
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/relationships/cities");
        }

        [TestMethod]
        public void GetRelationshipLink_returns_default_url_for_relationship_when_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"),
                "cities", typeof(City), null, null);
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.ResourceTypeName).Returns("countries");
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/relationships/cities");
        }

        [TestMethod]
        public void GetRelationshipLink_is_correct_if_template_is_present()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"),
                "cities", typeof(City), "foo/{1}/bar", null);
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/foo/45/bar");
        }

        [TestMethod]
        public void GetRelationshipLink_is_correct_if_template_is_present_and_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"),
                "cities", typeof(City), "foo/{1}/bar", null);
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelationshipLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/foo/45/bar");
        }

        [TestMethod]
        public void GetRelatedResourceLink_returns_default_url_for_relationship()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"),
                "cities", typeof(City), null, null);
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.ResourceTypeName).Returns("countries");
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/cities");
        }

        [TestMethod]
        public void GetRelatedResourceLink_returns_default_url_for_relationship_when_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"),
                "cities", typeof(City), null, null);
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.ResourceTypeName).Returns("countries");
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/countries/45/cities");
        }

        [TestMethod]
        public void GetRelatedResourceLink_is_correct_if_template_is_present()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"),
                "cities", typeof(City), null, "bar/{1}/qux");
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/bar/45/qux");
        }

        [TestMethod]
        public void GetRelatedResourceLink_is_correct_if_template_is_present_and_base_url_has_trailing_slash()
        {
            // Arrange
            var relationshipOwner = new Country { Id = "45" };
            var relationshipProperty = new ToManyResourceTypeRelationship(typeof(Country).GetProperty("Cities"),
                "cities", typeof(City), null, "bar/{1}/qux");
            var mockTypeRegistration = new Mock<IResourceTypeRegistration>(MockBehavior.Strict);
            mockTypeRegistration.Setup(r => r.GetIdForResource(relationshipOwner)).Returns("45");
            var mockRegistry = new Mock<IResourceTypeRegistry>(MockBehavior.Strict);
            mockRegistry.Setup(m => m.GetRegistrationForType(typeof(Country))).Returns(mockTypeRegistration.Object);

            // Act
            var conventions = new DefaultLinkConventions();
            var relationshipLink = conventions.GetRelatedResourceLink(relationshipOwner, mockRegistry.Object, relationshipProperty, "https://www.example.com");

            // Assert
            relationshipLink.Href.Should().Be("https://www.example.com/bar/45/qux");
        }

    }
}
