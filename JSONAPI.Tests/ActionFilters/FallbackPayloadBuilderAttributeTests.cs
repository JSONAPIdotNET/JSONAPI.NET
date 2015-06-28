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
using JSONAPI.Payload;
using JSONAPI.Payload.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace JSONAPI.Tests.ActionFilters
{
    [TestClass]
    public class FallbackPayloadBuilderAttributeTests
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
        public void OnActionExecutedAsync_leaves_ISingleResourcePayload_alone()
        {
            // Arrange
            var mockPayload = new Mock<ISingleResourcePayload>(MockBehavior.Strict);
            var actionExecutedContext = GetActionExecutedContext(mockPayload.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockPayload.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_IResourceCollectionPayload_alone()
        {
            // Arrange
            var mockPayload = new Mock<IResourceCollectionPayload>(MockBehavior.Strict);
            var actionExecutedContext = GetActionExecutedContext(mockPayload.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockPayload.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public void OnActionExecutedAsync_leaves_IErrorPayload_alone_but_changes_request_status_to_match_error_status()
        {
            // Arrange
            var mockError = new Mock<IError>(MockBehavior.Strict);
            mockError.Setup(e => e.Status).Returns(HttpStatusCode.Conflict);
            var mockPayload = new Mock<IErrorPayload>(MockBehavior.Strict);
            mockPayload.Setup(p => p.Errors).Returns(new[] {mockError.Object});
            var actionExecutedContext = GetActionExecutedContext(mockPayload.Object);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockPayload.Object);
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
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object);
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
        public void OnActionExecutedAsync_delegates_to_fallback_payload_builder_for_unknown_types()
        {
            // Arrange
            var payload = new Fruit();
            var actionExecutedContext = GetActionExecutedContext(payload);
            var cancellationTokenSource = new CancellationTokenSource();

            var mockResult = new Mock<IJsonApiPayload>(MockBehavior.Strict);
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);
            mockFallbackPayloadBuilder.Setup(b => b.BuildPayload(payload, It.IsAny<HttpRequestMessage>(), cancellationTokenSource.Token))
                .Returns(Task.FromResult(mockResult.Object));

            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);

            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            // Assert
            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockResult.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [TestMethod]
        public void OnActionExecutedAsync_creates_IErrorPayload_for_HttpError()
        {
            // Arrange
            var httpError = new HttpError("Some error");
            var actionExecutedContext = GetActionExecutedContext(httpError);
            var cancellationTokenSource = new CancellationTokenSource();
            var mockFallbackPayloadBuilder = new Mock<IFallbackPayloadBuilder>(MockBehavior.Strict);

            var mockError = new Mock<IError>(MockBehavior.Strict);
            mockError.Setup(e => e.Status).Returns(HttpStatusCode.OK);
            var mockResult = new Mock<IErrorPayload>(MockBehavior.Strict);
            mockResult.Setup(r => r.Errors).Returns(new[] { mockError.Object });

            var mockErrorPayloadBuilder = new Mock<IErrorPayloadBuilder>(MockBehavior.Strict);
            mockErrorPayloadBuilder.Setup(b => b.BuildFromHttpError(httpError, HttpStatusCode.OK)).Returns(mockResult.Object);
            
            // Act
            var attribute = new FallbackPayloadBuilderAttribute(mockFallbackPayloadBuilder.Object, mockErrorPayloadBuilder.Object);
            var task = attribute.OnActionExecutedAsync(actionExecutedContext, cancellationTokenSource.Token);
            task.Wait();

            // Assert
            ((ObjectContent)actionExecutedContext.Response.Content).Value.Should().BeSameAs(mockResult.Object);
            actionExecutedContext.Response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
