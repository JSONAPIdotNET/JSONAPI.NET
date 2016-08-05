using System;
using System.Net.Http;
using FluentAssertions;
using JSONAPI.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Http
{
    [TestClass]
    public class BaseUrlServiceTest
    {
        [TestMethod]
        public void BaseUrlRootTest()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService();

            // Act
            var baseUrl =baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/");
        }

        [TestMethod]
        public void BaseUrlOneLevelTest()
        {
            // Arrange
            const string uri = "http://api.example.com/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService("api");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/");
        }

        [TestMethod]
        public void BaseUrlOneLevelSlashTest()
        {
            // Arrange
            const string uri = "http://api.example.com/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService("/api");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/");
        }

        [TestMethod]
        public void BaseUrlOneLevelSlash2Test()
        {
            // Arrange
            const string uri = "http://api.example.com/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService("api/");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/");
        }

        [TestMethod]
        public void BaseUrlTwoLevelTest()
        {
            // Arrange
            const string uri = "http://api.example.com/api/superapi/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService("api/superapi");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/superapi/");
        }

        [TestMethod]
        public void BaseUrlTwoLevelSlashTest()
        {
            // Arrange
            const string uri = "http://api.example.com/api/superapi/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService("api/superapi/");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/superapi/");
        }

        [TestMethod]
        public void BaseUrlTwoLevelSlash2Test()
        {
            // Arrange
            const string uri = "http://api.example.com/api/superapi/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService("/api/superapi/");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/superapi/");
        }

        [TestMethod]
        public void BaseUrlConflictingNameTest()
        {
            // Arrange
            const string uri = "http://api.example.com/api/superapi?sort=api-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService("api");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/");
        }


        [TestMethod]
        public void BaseUrlPublicOriginTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("http://api.example.com/"), "");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginNoSlashTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("http://api.example.com"), "");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginHttpsTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("https://api.example.com/"), "");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("https://api.example.com/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginHttpsHighPortTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("https://api.example.com:12443/"), "");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("https://api.example.com:12443/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginInternalPortTest()
        {
            // Arrange
            const string uri = "http://wwwhost123:8080/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("http://api.example.com/"), "");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/");
        }



        [TestMethod]
        public void BaseUrlPublicOriginContextPathTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("http://api.example.com/"), "api");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginNoSlashContextPathTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("http://api.example.com"), "/api/");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginHttpsContextPathTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("https://api.example.com/"), "/api");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("https://api.example.com/api/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginHttpsHighPortContextPathTest()
        {
            // Arrange
            const string uri = "http://wwwhost123/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("https://api.example.com:12443/"), "api");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("https://api.example.com:12443/api/");
        }

        [TestMethod]
        public void BaseUrlPublicOriginInternalPortContextPathTest()
        {
            // Arrange
            const string uri = "http://wwwhost123:8080/api/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            var baseUrlService = new BaseUrlService(new Uri("http://api.example.com/"), "api");

            // Act
            var baseUrl = baseUrlService.GetBaseUrl(request);

            // Assert
            baseUrl.Should().BeEquivalentTo("http://api.example.com/api/");
        }


    }
}
