using System;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
        private static readonly Regex GuidRegex = new Regex(@"\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", RegexOptions.IgnoreCase);
        //private static readonly Regex StackTraceRegex = new Regex(@"""stackTrace"":[\s]*""[\w\:\\\.\s\,\-]*""");
        private static readonly Regex StackTraceRegex = new Regex(@"""stackTrace""[\s]*:[\s]*"".*?""");
        private static readonly Uri BaseUri = new Uri("http://localhost");

        protected static DbConnection GetEffortConnection()
        {
            return TestHelpers.GetEffortConnection(@"Acceptance\Data");
        }

        protected static async Task AssertResponseContent(HttpResponseMessage response, string expectedResponseTextResourcePath, HttpStatusCode expectedStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var expectedResponse =
                JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
            var redactedResponse = GuidRegex.Replace(responseContent, "{{SOME_GUID}}");
            redactedResponse = StackTraceRegex.Replace(redactedResponse, "\"stackTrace\":\"{{STACK_TRACE}}\"");

            redactedResponse.Should().Be(expectedResponse);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/vnd.api+json");
            response.Content.Headers.ContentType.CharSet.Should().Be("utf-8");

            response.StatusCode.Should().Be(expectedStatusCode);
        }

        #region GET

        protected async Task<HttpResponseMessage> SubmitGet(DbConnection effortConnection, string requestPath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var response = await server.CreateRequest(uri.ToString()).GetAsync();
                return response;
            }
        }

        #endregion
        #region POST

        protected async Task<HttpResponseMessage> SubmitPost(DbConnection effortConnection, string requestPath, string requestDataTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var requestContent = TestHelpers.ReadEmbeddedFile(requestDataTextResourcePath);
                var response = await server
                    .CreateRequest(uri.ToString())
                    .And(request =>
                    {
                        request.Content = new StringContent(requestContent, Encoding.UTF8, "application/vnd.api+json");
                    })
                    .PostAsync();
                return response;
            }
        }

        #endregion
        #region PATCH

        protected async Task<HttpResponseMessage> SubmitPatch(DbConnection effortConnection, string requestPath, string requestDataTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(context => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var requestContent = TestHelpers.ReadEmbeddedFile(requestDataTextResourcePath);
                var response = await server
                    .CreateRequest(uri.ToString())
                    .And(request =>
                    {
                        request.Content = new StringContent(requestContent, Encoding.UTF8, "application/vnd.api+json");
                    }).SendAsync("PATCH");
                return response;
            }
        }

        #endregion
        #region DELETE

        protected async Task<HttpResponseMessage> SubmitDelete(DbConnection effortConnection, string requestPath)
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
                return response;
            }
        }

        #endregion
    }
}
