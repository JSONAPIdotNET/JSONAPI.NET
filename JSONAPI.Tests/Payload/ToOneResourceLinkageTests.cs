using System.Linq;
using FluentAssertions;
using JSONAPI.Payload;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Payload
{
    [TestClass]
    public class ToOneResourceLinkageTests
    {
        [TestMethod]
        public void Returns_LinkageToken_for_present_identifier()
        {
            var mockIdentifier = new Mock<IResourceIdentifier>(MockBehavior.Strict);
            mockIdentifier.Setup(i => i.Type).Returns("countries");
            mockIdentifier.Setup(i => i.Id).Returns("1000");

            var linkage = new ToOneResourceLinkage(mockIdentifier.Object);

            linkage.LinkageToken.Should().BeOfType<JObject>();

            var obj = (JObject)linkage.LinkageToken;
            obj.Properties().Count().Should().Be(2);

            var type = (string)obj["type"];
            type.Should().Be("countries");

            var id = (string)obj["id"];
            id.Should().Be("1000");
        }

        [TestMethod]
        public void Returns_null_LinkageToken_for_missing_identifier()
        {
            var linkage = new ToOneResourceLinkage(null);

            linkage.LinkageToken.Should().BeNull();
        }
    }
}