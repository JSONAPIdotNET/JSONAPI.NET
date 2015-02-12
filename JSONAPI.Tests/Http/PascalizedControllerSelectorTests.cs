using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Dispatcher;
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
        private class FooBarBazQuxController
        {
            
        }

        private class TestHttpControllerTypeResolver : IHttpControllerTypeResolver
        {
            public ICollection<Type> GetControllerTypes(IAssembliesResolver assembliesResolver)
            {
                return new List<Type>
                {
                    typeof (FooBarBazQuxController)
                };
            }
        }

        [TestMethod]
        public void Selects_controller_for_all_lower_case_name()
        {
            TestForDefaultString("foobarbazqux");
        }

        [TestMethod]
        public void Selects_controller_for_name_with_underscores()
        {
            TestForDefaultString("foo_bar_baz_qux");
        }

        [TestMethod]
        public void Selects_controller_for_name_with_dots()
        {
            TestForDefaultString("foo.bar.baz.qux");
        }

        [TestMethod]
        public void Selects_controller_for_name_with_dashes()
        {
            TestForDefaultString("foo-bar-baz-qux");
        }

        [TestMethod]
        public void Selects_controller_for_name_with_all_three()
        {
            TestForDefaultString("foo.bar-baz_qux");
        }

        [TestMethod]
        public void Selects_controller_for_pascalized_name()
        {
            TestForDefaultString("FooBarBazQux");
        }

        [TestMethod]
        public void Selects_controller_for_all_caps_name()
        {
            TestForDefaultString("FOOBARBAZQUX");
        }

        private void TestForDefaultString(string defaultString)
        {
            // Arrange
            var routeDataDict = new Dictionary<string, object>
            {
                {"controller", defaultString}
            };

            var mockRouteData = new Mock<IHttpRouteData>(MockBehavior.Strict);
            mockRouteData.Setup(m => m.Route).Returns((IHttpRoute)null);
            mockRouteData.Setup(m => m.Values).Returns(routeDataDict);

            var httpConfig = new HttpConfiguration();
            httpConfig.Services.Replace(typeof(IHttpControllerTypeResolver), new TestHttpControllerTypeResolver());

            var mockRequestContext = new Mock<HttpRequestContext>(MockBehavior.Strict);
            mockRequestContext.Setup(m => m.Configuration).Returns(httpConfig);
            mockRequestContext.Setup(m => m.RouteData).Returns(mockRouteData.Object);

            var request = new HttpRequestMessage();
            request.SetRequestContext(mockRequestContext.Object);

            var selector = new PascalizedControllerSelector(httpConfig);

            // Act
            var actual = selector.SelectController(request);

            // Assert
            actual.ControllerType.Should().Be(typeof (FooBarBazQuxController));
        }
    }
}
