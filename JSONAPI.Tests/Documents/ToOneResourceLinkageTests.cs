using System.Linq;
using FluentAssertions;
using JSONAPI.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Documents
{
    [TestClass]
    public class ToOneResourceLinkageTests
    {
        [TestMethod]
        public void Identifiers_is_correct_for_present_identifier()
        {
            var mockIdentifier = new Mock<IResourceIdentifier>(MockBehavior.Strict);
            mockIdentifier.Setup(i => i.Type).Returns("countries");
            mockIdentifier.Setup(i => i.Id).Returns("1000");

            var linkage = new ToOneResourceLinkage(mockIdentifier.Object);

            linkage.Identifiers.Length.Should().Be(1);
            linkage.Identifiers.First().Type.Should().Be("countries");
            linkage.Identifiers.First().Id.Should().Be("1000");
        }

        [TestMethod]
        public void Identifiers_is_correct_for_missing_identifier()
        {
            var linkage = new ToOneResourceLinkage(null);

            linkage.Identifiers.Length.Should().Be(0);
        }
    }
}