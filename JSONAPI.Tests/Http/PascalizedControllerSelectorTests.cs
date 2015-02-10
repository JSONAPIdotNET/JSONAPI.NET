using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using FluentAssertions;
using JSONAPI.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.Http
{
    [TestClass]
    public class PascalizedControllerSelectorTests
    {
        [TestMethod]
        public void Camelizes_controller_name_without_dashes()
        {
            TestForDefaultString("foo", "Foo");
        }

        [TestMethod]
        public void Camelizes_controller_name_with_underscores()
        {
            TestForDefaultString("foo_bar_baz", "FooBarBaz");
        }

        [TestMethod]
        public void Camelizes_controller_name_with_dots()
        {
            TestForDefaultString("foo.bar.baz", "FooBarBaz");
        }

        [TestMethod]
        public void Camelizes_controller_name_with_dashes()
        {
            TestForDefaultString("foo-bar-baz", "FooBarBaz");
        }

        [TestMethod]
        public void Camelizes_controller_name_with_all_three()
        {
            TestForDefaultString("foo.bar-baz_qux", "FooBarBazQux");
        }

        private void TestForDefaultString(string defaultString, string expectedTransformation)
        {
            // Arrange
            var routeDataDict = new Dictionary<string, object>
            {
                {"controller", defaultString}
            };

            var mockRouteData = new Mock<IHttpRouteData>(MockBehavior.Strict);
            mockRouteData.Setup(m => m.Values).Returns(routeDataDict);

            var mockRequestContext = new Mock<HttpRequestContext>(MockBehavior.Strict);
            mockRequestContext.Setup(m => m.RouteData).Returns(mockRouteData.Object);

            var request = new HttpRequestMessage();
            request.SetRequestContext(mockRequestContext.Object);

            var mockHttpConfig = new Mock<HttpConfiguration>(MockBehavior.Strict);

            var selector = new PascalizedControllerSelector(mockHttpConfig.Object);

            // Act
            var actual = selector.GetControllerName(request);

            // Assert
            actual.Should().Be(expectedTransformation);
        }
    }
}
