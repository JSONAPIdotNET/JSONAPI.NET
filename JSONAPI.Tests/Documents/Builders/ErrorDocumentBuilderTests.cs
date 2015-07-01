using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;

namespace JSONAPI.Tests.Documents.Builders
{
    [TestClass]
    public class ErrorDocumentBuilderTests
    {
        private const string GuidRegex = @"\b[a-f0-9]{8}(?:-[a-f0-9]{4}){3}-[a-f0-9]{12}\b";

        [TestMethod]
        public void Builds_document_from_exception()
        {
            // Arrange
            Exception theException;
            try
            {
                throw new Exception("This is the exception!");
            }
            catch (Exception ex)
            {
                theException = ex;
            }

            // Act
            var errorDocumentBuilder = new ErrorDocumentBuilder();
            var document = errorDocumentBuilder.BuildFromException(theException);

            // Assert
            document.Errors.Length.Should().Be(1);
            var error = document.Errors.First();
            error.Id.Should().MatchRegex(GuidRegex);
            error.Title.Should().Be("Unhandled exception");
            error.Detail.Should().Be("An unhandled exception was thrown while processing the request.");
            error.Status.Should().Be(HttpStatusCode.InternalServerError);
            ((string)error.Metadata.MetaObject["exceptionMessage"]).Should().Be("This is the exception!");
            ((string)error.Metadata.MetaObject["stackTrace"]).Should().NotBeNull();
        }

        [TestMethod]
        public void Builds_document_from_exception_with_inner_exception()
        {
            // Arrange
            Exception theException;
            try
            {
                try
                {
                    throw new Exception("This is the inner exception!");
                }
                catch (Exception ex)
                {
                    throw new Exception("This is the outer exception!", ex);
                }
            }
            catch (Exception ex)
            {
                theException = ex;
            }

            // Act
            var errorDocumentBuilder = new ErrorDocumentBuilder();
            var document = errorDocumentBuilder.BuildFromException(theException);

            // Assert
            document.Errors.Length.Should().Be(1);
            var error = document.Errors.First();
            error.Id.Should().MatchRegex(GuidRegex);
            error.Title.Should().Be("Unhandled exception");
            error.Detail.Should().Be("An unhandled exception was thrown while processing the request.");
            error.Status.Should().Be(HttpStatusCode.InternalServerError);
            ((string)error.Metadata.MetaObject["exceptionMessage"]).Should().Be("This is the outer exception!");
            ((string)error.Metadata.MetaObject["stackTrace"]).Should().NotBeNull();

            var inner = (JObject)error.Metadata.MetaObject["innerException"];
            ((string)inner["exceptionMessage"]).Should().Be("This is the inner exception!");
            ((string)inner["stackTrace"]).Should().NotBeNull();
        }

        [TestMethod]
        public void Builds_document_from_exception_with_two_levels_deep_inner_exception()
        {
            // Arrange
            Exception theException;
            try
            {
                try
                {
                    try
                    {
                        throw new Exception("This is the inner exception!");
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("This is the middle exception!", ex);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("This is the outer exception!", ex);
                }
            }
            catch (Exception ex)
            {
                theException = ex;
            }

            // Act
            var errorDocumentBuilder = new ErrorDocumentBuilder();
            var document = errorDocumentBuilder.BuildFromException(theException);

            // Assert
            document.Errors.Length.Should().Be(1);
            var error = document.Errors.First();
            error.Id.Should().MatchRegex(GuidRegex);
            error.Title.Should().Be("Unhandled exception");
            error.Detail.Should().Be("An unhandled exception was thrown while processing the request.");
            error.Status.Should().Be(HttpStatusCode.InternalServerError);
            ((string)error.Metadata.MetaObject["exceptionMessage"]).Should().Be("This is the outer exception!");
            ((string)error.Metadata.MetaObject["stackTrace"]).Should().NotBeNull();

            var middle = (JObject)error.Metadata.MetaObject["innerException"];
            ((string)middle["exceptionMessage"]).Should().Be("This is the middle exception!");
            ((string)middle["stackTrace"]).Should().NotBeNull();

            var inner = (JObject)middle["innerException"];
            ((string)inner["exceptionMessage"]).Should().Be("This is the inner exception!");
            ((string)inner["stackTrace"]).Should().NotBeNull();
        }

        [TestMethod]
        public void Builds_document_from_JsonApiException()
        {
            // Arrange
            var mockError = new Mock<IError>(MockBehavior.Strict);
            JsonApiException theException;
            try
            {
                throw new JsonApiException(mockError.Object);
            }
            catch (JsonApiException ex)
            {
                theException = ex;
            }

            // Act
            var errorDocumentBuilder = new ErrorDocumentBuilder();
            var document = errorDocumentBuilder.BuildFromException(theException);

            // Assert
            document.Errors.Length.Should().Be(1);
            document.Errors.First().Should().Be(mockError.Object);
        }
    }
}
