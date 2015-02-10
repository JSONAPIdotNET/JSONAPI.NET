using System;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.EntityFramework.Tests.TestWebApp;
using JSONAPI.EntityFramework.Tests.TestWebApp.Models;
using JSONAPI.Json;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.EntityFramework.Tests.Acceptance
{
    [TestClass]
    public abstract class AcceptanceTestsBase
    {
        private static readonly Uri BaseUri = new Uri("http://localhost");

        protected static DbConnection GetEffortConnection()
        {
            return TestHelpers.GetEffortConnection(@"Acceptance\Data");
        }

        protected async Task TestGet(DbConnection effortConnection, string requestPath, string expectedResponseTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var response = await server.CreateRequest(uri.ToString()).GetAsync();
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseContent = await response.Content.ReadAsStringAsync();

                var expected =
                    JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
                responseContent.Should().Be(expected);
            }
        }

        protected async Task TestGetWithFilter(DbConnection effortConnection, string requestPath, string expectedResponseTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var response = await server.CreateRequest(uri.ToString()).GetAsync();
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseContent = await response.Content.ReadAsStringAsync();

                var expected =
                    JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
                responseContent.Should().Be(expected);
            }
        }

        protected async Task TestGetById(DbConnection effortConnection, string requestPath, string expectedResponseTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var response = await server.CreateRequest(uri.ToString()).GetAsync();
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseContent = await response.Content.ReadAsStringAsync();

                var expected =
                    JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
                responseContent.Should().Be(expected);
            }
        }

        protected async Task TestPost(DbConnection effortConnection, string requestPath, string requestDataTextResourcePath, string expectedResponseTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var requestContent =
                    JsonHelpers.MinifyJson(
                        TestHelpers.ReadEmbeddedFile(requestDataTextResourcePath));
                var response = await server
                    .CreateRequest(uri.ToString())
                    .And(request =>
                    {
                        request.Content = new StringContent(requestContent, Encoding.UTF8, "application/vnd.api+json");
                    })
                    .PostAsync();
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseContent = await response.Content.ReadAsStringAsync();

                var expected =
                    JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
                responseContent.Should().Be(expected);
            }
        }

        protected async Task TestPut(DbConnection effortConnection, string requestPath, string requestDataTextResourcePath, string expectedResponseTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var requestContent =
                    JsonHelpers.MinifyJson(
                        TestHelpers.ReadEmbeddedFile(requestDataTextResourcePath));
                var response = await server
                    .CreateRequest(uri.ToString())
                    .And(request =>
                    {
                        request.Content = new StringContent(requestContent, Encoding.UTF8,
                            "application/vnd.api+json");
                    }).SendAsync("PUT");
                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var responseContent = await response.Content.ReadAsStringAsync();

                var expected =
                    JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
                responseContent.Should().Be(expected);
            }
        }

        protected async Task TestDelete(DbConnection effortConnection, string requestPath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var response = await server
                    .CreateRequest(uri.ToString())
                    .SendAsync("DELETE");
                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
            }
        }
    }
}
