using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using FluentAssertions;
using JSONAPI.ActionFilters;
using JSONAPI.Documents;
using JSONAPI.Documents.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.ActionFilters
{
    [TestClass]
    public class FallbackDocumentBuilderAttributeTests
    {
        private HttpActionExecutedContext GetActionExecutedContext(object objectContentValue, Exception exception = null)
        {
            var mockMediaTypeFormatter = new Mock<MediaTypeFormatter>(MockBehavior.Strict);
            mockMediaTypeFormatter.Setup(f => f.CanWriteType(It.IsAny<Type>())).Returns(true);
            mockMediaTypeFormatter.Setup(f => f.SetDefaultContentHeaders(It.IsAny<Type>(), It.IsAny<HttpContentHeaders>(), It.IsAny<MediaTypeHeaderValue>()));
            var response = new HttpResponseMessage
            {
                Content = new ObjectContent(objectContentValue.GetType(), objectContentValue, mockMediaTypeFormatter.Object)
            };
            var actionContext = new HttpActionContext { Response = response };
            return new HttpActionExecutedContext(actionContext, exception);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_ISingleResourceDocument_alone()
        {
            // Arrange
            var mockDocument = new Mock<ISingleResourceDocument>(MockBehavior.Strict);
            var actionExecutedContext = GetActionExecutedContext(mockDocument.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackDocumentBuilder = new Mock<IFallbackDocumentBuilder>(MockBehavior.Strict);
            var mockErrorDocumentBuilder = new Mock<IErrorDocumentBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackDocumentBuilderAttribute(mockFallbackDocumentBuilder.Object, mockErrorDocumentBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockDocument.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_IResourceCollectionDocument_alone()
        {
            // Arrange
            var mockDocument = new Mock<IResourceCollectionDocument>(MockBehavior.Strict);
            var actionExecutedContext = GetActionExecutedContext(mockDocument.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackDocumentBuilder = new Mock<IFallbackDocumentBuilder>(MockBehavior.Strict);
            var mockErrorDocumentBuilder = new Mock<IErrorDocumentBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackDocumentBuilderAttribute(mockFallbackDocumentBuilder.Object, mockErrorDocumentBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockDocument.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_IErrorDocument_alone_but_changes_request_status_to_match_error_status()
        {
            // Arrange
            var mockError = new Mock<IError>(MockBehavior.Strict);
            mockError.Setup(e => e.Status).Returns(HttpStatusCode.Conflict);
            var mockDocument = new Mock<IErrorDocument>(MockBehavior.Strict);
            mockDocument.Setup(p => p.Errors).Returns(new[] {mockError.Object});
            var actionExecutedContext = GetActionExecutedContext(mockDocument.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackDocumentBuilder = new Mock<IFallbackDocumentBuilder>(MockBehavior.Strict);
            var mockErrorDocumentBuilder = new Mock<IErrorDocumentBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackDocumentBuilderAttribute(mockFallbackDocumentBuilder.Object, mockErrorDocumentBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockDocument.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [TestMethod]
        public void OnActionExecutedAsync_does_nothing_if_there_is_an_exception()
        {
            // Arrange
            var objectContent = new object();
            var theException = new Exception("This is an error.");
            var actionExecutedContext = GetActionExecutedContext(objectContent, theException);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackDocumentBuilder = new Mock<IFallbackDocumentBuilder>(MockBehavior.Strict);
            var mockErrorDocumentBuilder = new Mock<IErrorDocumentBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackDocumentBuilderAttribute(mockFallbackDocumentBuilder.Object, mockErrorDocumentBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            var newObjectContent = ((ObjectContent) actionExecutedContext.Response.Content).Value;
            newObjectContent.Should().BeSameAs(objectContent);
            actionExecutedContext.Exception.Should().Be(theException);
        }

        private class Fruit
        {
        }

        [TestMethod]
        public void OnActionExecutedAsync_delegates_to_fallback_document_builder_for_unknown_types()
        {
            // Arrange
            var resource = new Fruit();
            var actionExecutedContext = GetActionExecutedContext(resource);
            var cancellationTokenSource = new CancellationTokenSource();

            var mockResult = new Mock<IJsonApiDocument>(MockBehavior.Strict);
            var mockFallbackDocumentBuilder = new Mock<IFallbackDocumentBuilder>(MockBehavior.Strict);
            mockFallbackDocumentBuilder.Setup(b => b.BuildDocument(resource, It.IsAny<HttpRequestMessage>(), cancellationTokenSource.Token))
                .Returns(Task.FromResult(mockResult.Object));

            var mockErrorDocumentBuilder = new Mock<IErrorDocumentBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackDocumentBuilderAttribute(mockFallbackDocumentBuilder.Object, mockErrorDocumentBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            // Assert
            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockResult.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public void OnActionExecutedAsync_creates_IErrorDocument_for_HttpError()
        {
            // Arrange
            var httpError = new HttpError("Some error");
            var actionExecutedContext = GetActionExecutedContext(httpError);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackDocumentBuilder = new Mock<IFallbackDocumentBuilder>(MockBehavior.Strict);

            var mockError = new Mock<IError>(MockBehavior.Strict);
            mockError.Setup(e => e.Status).Returns(HttpStatusCode.OK);
            var mockResult = new Mock<IErrorDocument>(MockBehavior.Strict);
            mockResult.Setup(r => r.Errors).Returns(new[] { mockError.Object });

            var mockErrorDocumentBuilder = new Mock<IErrorDocumentBuilder>(MockBehavior.Strict);
            mockErrorDocumentBuilder.Setup(b => b.BuildFromHttpError(httpError, HttpStatusCode.OK)).Returns(mockResult.Object);
            
            // Act
            var attribute = new FallbackDocumentBuilderAttribute(mockFallbackDocumentBuilder.Object, mockErrorDocumentBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            // Assert
            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockResult.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
