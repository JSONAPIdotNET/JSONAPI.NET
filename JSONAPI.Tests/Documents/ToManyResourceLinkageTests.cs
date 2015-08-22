using FluentAssertions;
using JSONAPI.Documents;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Documents
{
    [TestClass]
    public class ToManyResourceLinkageTests
    {
        [TestMethod]
        public void Identifiers_is_correct_for_present_identifiers()
        {
            var mockIdentifier1 = new Mock<IResourceIdentifier>(MockBehavior.Strict);
            mockIdentifier1.Setup(i => i.Type).Returns("countries");
            mockIdentifier1.Setup(i => i.Id).Returns("1000");

            var mockIdentifier2 = new Mock<IResourceIdentifier>(MockBehavior.Strict);
            mockIdentifier2.Setup(i => i.Type).Returns("cities");
            mockIdentifier2.Setup(i => i.Id).Returns("4000");

            var linkage = new ToManyResourceLinkage(new [] { mockIdentifier1.Object, mockIdentifier2.Object });

            linkage.Identifiers.Length.Should().Be(2);
            linkage.Identifiers[0].Type.Should().Be("countries");
            linkage.Identifiers[0].Id.Should().Be("1000");
            linkage.Identifiers[1].Type.Should().Be("cities");
            linkage.Identifiers[1].Id.Should().Be("4000");
        }

        [TestMethod]
        public void Returns_corrent_LinkageToken_for_null_identifiers()
        {
            var linkage = new ToManyResourceLinkage(null);

            linkage.Identifiers.Length.Should().Be(0);
        }

        [TestMethod]
        public void Returns_corrent_LinkageToken_for_empty_identifiers()
        {
            var linkage = new ToManyResourceLinkage(new IResourceIdentifier[] { });

            linkage.Identifiers.Length.Should().Be(0);
        }
    }
}