using System;
using System.Data.Common;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
using JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Models;
using JSONAPI.Json;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Owin;

namespace JSONAPI.AcceptanceTests.EntityFrameworkTestWebApp.Tests
{
    [TestClass]
    public abstract class AcceptanceTestsBase
    {
        private const string JsonApiContentType = "application/vnd.api+json";
        private static readonly Regex GuidRegex = new Regex(@"\b[A-F0-9]{8}(?:-[A-F0-9]{4}){3}-[A-F0-9]{12}\b", RegexOptions.IgnoreCase);
        //private static readonly Regex StackTraceRegex = new Regex(@"""stackTrace"":[\s]*""[\w\:\\\.\s\,\-]*""");
        private static readonly Regex StackTraceRegex = new Regex(@"""stackTrace""[\s]*:[\s]*"".*?""");
        protected static Uri BaseUri = new Uri("https://www.example.com");

        protected static DbConnection GetEffortConnection()
        {
            return TestHelpers.GetEffortConnection(@"Data");
        }

        protected virtual async Task AssertResponseContent(HttpResponseMessage response, string expectedResponseTextResourcePath, HttpStatusCode expectedStatusCode, bool redactErrorData = false)
        {
            var responseContent = await response.Content.ReadAsStringAsync();

            var expectedResponse = ExpectedResponse(expectedResponseTextResourcePath);
            string actualResponse;
            if (redactErrorData)
            {
                var redactedResponse = GuidRegex.Replace(responseContent, "{{SOME_GUID}}");
                actualResponse = StackTraceRegex.Replace(redactedResponse, "\"stackTrace\":\"{{STACK_TRACE}}\"");
                actualResponse.Should().Be(expectedResponse);
            }
            else
            {
                actualResponse = responseContent;
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff+00:00",
                    Culture = CultureInfo.InvariantCulture,
                    Formatting = Formatting.Indented
                };

                var actualResponseJObject = JsonConvert.DeserializeObject(actualResponse) as JObject;
                var expectedResponseJObject = JsonConvert.DeserializeObject(expectedResponse) as JObject;
                var equals = JToken.DeepEquals(actualResponseJObject, expectedResponseJObject);
                if (!equals)
                {
                    Assert.Fail("should be: " + JsonConvert.SerializeObject(expectedResponseJObject, settings) + "\n but was: " + JsonConvert.SerializeObject(actualResponseJObject, settings));
                }
            }

            response.Content.Headers.ContentType.MediaType.Should().Be(JsonApiContentType);
            response.Content.Headers.ContentType.CharSet.Should().Be("utf-8");

            response.StatusCode.Should().Be(expectedStatusCode);
        }

        protected virtual string ExpectedResponse(string expectedResponseTextResourcePath)
        {
            var expectedResponse =
                JsonHelpers.MinifyJson(TestHelpers.ReadEmbeddedFile(expectedResponseTextResourcePath));
            return expectedResponse;
        }

        #region GET

        protected async Task<HttpResponseMessage> SubmitGet(DbConnection effortConnection, string requestPath)
        {
            using (var server = TestServer.Create(app =>
            {
                var startup = new Startup(() => new TestDbContext(effortConnection, false));
                StartupConfiguration(startup, app);
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
                StartupConfiguration(startup, app);
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
                StartupConfiguration(startup, app);
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
                StartupConfiguration(startup, app);
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

 

        #region configure startup

        /// <summary>
        /// Startup process was divided into 4 steps to support better acceptance tests.
        /// This method can be overridden by subclass to change behavior of setup.
        /// </summary>
        /// <param name="startup"></param>
        /// <param name="app"></param>
        protected virtual void StartupConfiguration(Startup startup, IAppBuilder app)
        {
            startup.Configuration(app);
        }

        #endregion
    }
}
