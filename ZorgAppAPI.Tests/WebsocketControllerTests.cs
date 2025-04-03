using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZorgAppAPI.Controllers;

namespace ZorgAppAPI.Tests
{
    [TestClass]
    public class WebSocketControllerTests
    {
        private Mock<ILogger<WebSocketController>> _mockLogger;
        private WebSocketController _webSocketController;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<WebSocketController>>();
            _webSocketController = new WebSocketController(_mockLogger.Object);
        }

        [TestMethod]
        public async Task Get_NonWebSocketRequest_ReturnsBadRequest()
        {
            // Arrange
            var context = new DefaultHttpContext();
            _webSocketController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            await _webSocketController.Get();

            // Assert
            Assert.AreEqual(400, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task Get_UserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Features.Set<IHttpWebSocketFeature>(new TestWebSocketFeature());
            _webSocketController.ControllerContext = new ControllerContext
            {
                HttpContext = context
            };

            // Act
            await _webSocketController.Get();

            // Assert
            Assert.AreEqual(401, context.Response.StatusCode);
        }

        [TestMethod]
        public async Task SendToUserAsync_UserNotConnected_ReturnsFalse()
        {
            // Act
            var result = await WebSocketController.SendToUserAsync("nonexistentUser", new { message = "test" });

            // Assert
            Assert.IsFalse(result);
        }


        private class TestWebSocketFeature : IHttpWebSocketFeature
        {
            public bool IsWebSocketRequest => true;

            public IList<string> WebSocketRequestedProtocols => new List<string>();

            public Task<WebSocket> AcceptAsync(WebSocketAcceptContext context)
            {
                return Task.FromResult<WebSocket>(new TestWebSocket());
            }
        }

        private class TestWebSocket : WebSocket
        {
            public override WebSocketCloseStatus? CloseStatus => WebSocketCloseStatus.NormalClosure;
            public override string CloseStatusDescription => "Closed";
            public override WebSocketState State => WebSocketState.Open;
            public override string SubProtocol => "test";

            public override void Abort() { }

            public override Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }

            public override void Dispose() { }

            public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
            {
                var message = Encoding.UTF8.GetBytes("test message");
                buffer = new ArraySegment<byte>(message);
                return Task.FromResult(new WebSocketReceiveResult(message.Length, WebSocketMessageType.Text, true));
            }

            public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
            {
                return Task.CompletedTask;
            }
        }
    }
}

