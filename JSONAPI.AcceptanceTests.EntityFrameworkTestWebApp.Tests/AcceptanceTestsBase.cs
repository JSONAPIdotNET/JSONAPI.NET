using System;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.Json;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public abstract class AcceptanceTestsBase
    {
        private const string JsonApiContentType = "application/vnd.api+json";
        private static readonly Regex GuidRegex = new Regex(@"\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", RegexOptions.IgnoreCase);
        //private static readonly Regex StackTraceRegex = new Regex(@"""stackTrace"":[\s]*""[\w\:\\\.\s\,\-]*""");
        private static readonly Regex StackTraceRegex = new Regex(@"""stackTrace""[\s]*:[\s]*"".*?""");
        private static readonly Uri BaseUri = new Uri("https://www.example.com");

        protected static DbConnection GetEffortConnection()
        {
            return TestHelpers.GetEffortConnection(@"Data");
        }

        protected static async Task AssertResponseContent(HttpResponseMessage response, string expectedResponseTextResourcePath, HttpStatusCode expectedStatusCode, bool redactErrorData = false)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var expectedResponse =
                JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
            string actualResponse;
            if (redactErrorData)
            {
                var redactedResponse = GuidRegex.Replace(responseContent, "{{SOME_GUID}}");
                actualResponse = StackTraceRegex.Replace(redactedResponse, "\"stackTrace\":\"{{STACK_TRACE}}\"");
            }
            else
            {
                actualResponse = responseContent;
            }

            actualResponse.Should().Be(expectedResponse);
            response.Content.Headers.ContentType.MediaType.Should().Be(JsonApiContentType);
            response.Content.Headers.ContentType.CharSet.Should().Be("utf-8");

            response.StatusCode.Should().Be(expectedStatusCode);
        }

        #region GET

        protected async Task<HttpResponseMessage> SubmitGet(DbConnection effortConnection, string requestPath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(() => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var response = await server.CreateRequest(uri.ToString()).AddHeader("Accept", JsonApiContentType).GetAsync();
                return response;
            }
        }

        #endregion
        #region POST

        protected async Task<HttpResponseMessage> SubmitPost(DbConnection effortConnection, string requestPath, string requestDataTextResourcePath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(() => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var requestContent = TestHelpers.ReadEmbeddedFile(requestDataTextResourcePath);
                var response = await server
                    .CreateRequest(uri.ToString())
                    .AddHeader("Accept", JsonApiContentType)
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
                var startup = new Startup(() => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var requestContent = TestHelpers.ReadEmbeddedFile(requestDataTextResourcePath);
                var response = await server
                    .CreateRequest(uri.ToString())
                    .AddHeader("Accept", JsonApiContentType)
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
                var startup = new Startup(() => new TestDbContext(effortConnection, false));
                startup.Configuration(app);
            }))
            {
                var uri = new Uri(BaseUri, requestPath);
                var response = await server
                    .CreateRequest(uri.ToString())
                    .AddHeader("Accept", JsonApiContentType)
                    .SendAsync("DELETE");
                return response;
            }
        }

        #endregion
    }
}
