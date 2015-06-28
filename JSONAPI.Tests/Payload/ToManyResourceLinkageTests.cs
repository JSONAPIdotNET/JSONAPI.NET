using System.Linq;
using FluentAssertions;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class ToManyResourceLinkageTests
    {
        [TestMethod]
        public void Returns_corrent_LinkageToken_for_present_identifiers()
        {
            var mockIdentifier1 = new Mock<IResourceIdentifier>(MockBehavior.Strict);
            mockIdentifier1.Setup(i => i.Type).Returns("countries");
            mockIdentifier1.Setup(i => i.Id).Returns("1000");

            var mockIdentifier2 = new Mock<IResourceIdentifier>(MockBehavior.Strict);
            mockIdentifier2.Setup(i => i.Type).Returns("cities");
            mockIdentifier2.Setup(i => i.Id).Returns("4000");

            var linkage = new ToManyResourceLinkage(new [] { mockIdentifier1.Object, mockIdentifier2.Object });

            linkage.LinkageToken.Should().BeOfType<JArray>();

            var arr = (JArray)linkage.LinkageToken;
            arr.Count.Should().Be(2);

            var item1 = arr[0];
            ((string)item1["type"]).Should().Be("countries");
            ((string)item1["id"]).Should().Be("1000");

            var item2 = arr[1];
            ((string)item2["type"]).Should().Be("cities");
            ((string)item2["id"]).Should().Be("4000");
        }

        [TestMethod]
        public void Returns_corrent_LinkageToken_for_null_identifiers()
        {
            var linkage = new ToManyResourceLinkage(null);

            linkage.LinkageToken.Should().BeOfType<JArray>();
            linkage.LinkageToken.Count().Should().Be(0);
        }

        [TestMethod]
        public void Returns_corrent_LinkageToken_for_empty_identifiers()
        {
            var linkage = new ToManyResourceLinkage(new IResourceIdentifier[] { });

            linkage.LinkageToken.Should().BeOfType<JArray>();
            linkage.LinkageToken.Count().Should().Be(0);
        }
    }
}