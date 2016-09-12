using System.Net.Http;
using FluentAssertions;
using JSONAPI.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JSONAPI.Tests.Http
{
    [TestClass]
    public class DefaultIncludeExpressionExtractorTests
    {
        [TestMethod]
        public void ExtractsSingleIncludeExpressionFromUri()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies?include=boss";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultIncludeExpressionExtractor();
            var inclExpressions = extractor.ExtractIncludeExpressions(request);

            // Assert
            inclExpressions.Should().BeEquivalentTo("boss");
        }


        [TestMethod]
        public void ExtractsMultipleIncludeExpressionsFromUri()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies?include=boss,office-address";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultIncludeExpressionExtractor();
            var inclExpressions = extractor.ExtractIncludeExpressions(request);

            // Assert
            inclExpressions.Should().BeEquivalentTo("boss", "office-address");
        }

        [TestMethod]
        public void ExtractsNothingWhenThereIsNoIncludeParam()
        {
            // Arrange
            const string uri = "http://api.example.com/dummies";
            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            // Act
            var extractor = new DefaultIncludeExpressionExtractor();
            var inclExpression = extractor.ExtractIncludeExpressions(request);

            // Assert
            inclExpression.Length.Should().Be(0);
        }
    }
}
