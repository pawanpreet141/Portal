//using System.Text.Json;
//using Employee.API.Middleware;
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Moq;
//using Xunit;

//namespace Employee.Tests
//{
//    public class GlobalExceptionHandlerTests
//    {
//        private readonly Mock<ILogger<GlobalExceptionHandler>>
//            _loggerMock;

//        private readonly GlobalExceptionHandler _handler;

//        public GlobalExceptionHandlerTests()
//        {
//            _loggerMock =
//                new Mock<ILogger<GlobalExceptionHandler>>();

//            _handler =
//                new GlobalExceptionHandler(_loggerMock.Object);
//        }

//        [Fact]
//        public async Task TryHandleAsync_ShouldReturnTrue_WhenExceptionIsHandled()
//        {
//            // Arrange
//            var context = new DefaultHttpContext();

//            context.Response.Body = new MemoryStream();

//            var exception =
//                new Exception("Something went wrong");

//            // Act
//            var result = await _handler.TryHandleAsync(
//                context,
//                exception,
//                CancellationToken.None);

//            // Assert
//            Assert.True(result);
//        }

//        [Fact]
//        public async Task TryHandleAsync_ShouldReturn500StatusCode()
//        {
//            // Arrange
//            var context = new DefaultHttpContext();

//            context.Response.Body = new MemoryStream();

//            var exception =
//                new Exception("Test exception");

//            // Act
//            await _handler.TryHandleAsync(
//                context,
//                exception,
//                CancellationToken.None);

//            // Assert
//            Assert.Equal(
//                StatusCodes.Status500InternalServerError,
//                context.Response.StatusCode);
//        }

//        [Fact]
//        public async Task TryHandleAsync_ShouldReturnProblemJsonContentType()
//        {
//            // Arrange
//            var context = new DefaultHttpContext();

//            context.Response.Body = new MemoryStream();

//            var exception =
//                new Exception("Test exception");

//            // Act
//            await _handler.TryHandleAsync(
//                context,
//                exception,
//                CancellationToken.None);

//            // Assert
//            Assert.Equal(
//                "application/problem+json",
//                context.Response.ContentType);
//        }

//        [Fact]
//        public async Task TryHandleAsync_ShouldReturnProblemDetailsResponse()
//        {
//            // Arrange
//            var context = new DefaultHttpContext();

//            context.Request.Path = "/api/employee";

//            context.Response.Body = new MemoryStream();

//            var exception =
//                new Exception("Database error");

//            // Act
//            await _handler.TryHandleAsync(
//                context,
//                exception,
//                CancellationToken.None);

//            // Assert
//            context.Response.Body.Position = 0;

//            using var document =
//                await JsonDocument.ParseAsync(
//                    context.Response.Body);

//            var root = document.RootElement;

//            Assert.Equal(
//                500,
//                root.GetProperty("status").GetInt32());

//            Assert.Equal(
//                "An unexpected error occurred.",
//                root.GetProperty("title").GetString());

//            Assert.Equal(
//                "Database error",
//                root.GetProperty("detail").GetString());

//            Assert.Equal(
//                "/api/employee",
//                root.GetProperty("instance").GetString());

//            Assert.True(root.TryGetProperty(
//                "traceId", out _));
//        }

//        [Fact]
//        public async Task TryHandleAsync_ShouldReturnFalse_WhenResponseHasStarted()
//        {
//            // Arrange
//            var context = new DefaultHttpContext();

//            context.Response.Body = new MemoryStream();

//            context.Response.OnStarting(() => Task.CompletedTask);

//            await context.Response.StartAsync();

//            var exception =
//                new Exception("Test exception");

//            // Act
//            var result = await _handler.TryHandleAsync(
//                context,
//                exception,
//                CancellationToken.None);

//            // Assert
//            Assert.False(result);
//        }

//        [Fact]
//        public async Task TryHandleAsync_ShouldLogException()
//        {
//            // Arrange
//            var context = new DefaultHttpContext();

//            context.Response.Body = new MemoryStream();

//            var exception =
//                new Exception("Test logging");

//            // Act
//            await _handler.TryHandleAsync(
//                context,
//                exception,
//                CancellationToken.None);

//            // Assert
//            _loggerMock.Verify(
//                x => x.Log(
//                    LogLevel.Error,
//                    It.IsAny<EventId>(),
//                    It.Is<It.IsAnyType>((v, t) =>
//                        v.ToString()!.Contains(
//                            "Unhandled exception occurred")),
//                    exception,
//                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
//                Times.Once);
//        }
//    }
//}

