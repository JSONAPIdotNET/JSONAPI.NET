using System.Net.Http;
using FluentAssertions;
using JSONAPI.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Http
{
    [TestClass]
    public class DefaultSortExpressionExtractorTests
    {
        [TestMethod]
        public void ExtractsSingleSortExpressionFromUri()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies?sort=first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultSortExpressionExtractor();
            var sortExpressions = extractor.ExtractSortExpressions(request);

            // Assert
            sortExpressions.Should().BeEquivalentTo("first-name");
        }

        [TestMethod]
        public void ExtractsSingleDescendingSortExpressionFromUri()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies?sort=-first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultSortExpressionExtractor();
            var sortExpressions = extractor.ExtractSortExpressions(request);

            // Assert
            sortExpressions.Should().BeEquivalentTo("-first-name");
        }

        [TestMethod]
        public void ExtractsMultipleSortExpressionsFromUri()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies?sort=last-name,first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultSortExpressionExtractor();
            var sortExpressions = extractor.ExtractSortExpressions(request);

            // Assert
            sortExpressions.Should().BeEquivalentTo("last-name", "first-name");
        }

        [TestMethod]
        public void ExtractsMultipleSortExpressionsFromUriWithDifferentDirections()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies?sort=last-name,-first-name";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultSortExpressionExtractor();
            var sortExpressions = extractor.ExtractSortExpressions(request);

            // Assert
            sortExpressions.Should().BeEquivalentTo("last-name", "-first-name");
        }

        [TestMethod]
        public void ExtractsNothingWhenThereIsNoSortParam()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultSortExpressionExtractor();
            var sortExpressions = extractor.ExtractSortExpressions(request);

            // Assert
            sortExpressions.Length.Should().Be(0);
        }
    }
}
