using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly ILogger<WebSocketController> _logger;
        private static readonly ConcurrentDictionary<string, WebSocket> _connectedClients = new();

        public WebSocketController(ILogger<WebSocketController> logger)
        {
            _logger = logger;
        }

        [HttpGet("/ws")]
        [Authorize]
        public async Task Get()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                _logger.LogWarning("Non-WebSocket request received");
                HttpContext.Response.StatusCode = 400;
                await HttpContext.Response.WriteAsync("This endpoint accepts WebSocket requests only.");
                return;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("User ID not found in claims");
                HttpContext.Response.StatusCode = 401;
                await HttpContext.Response.WriteAsync("Unauthorized access.");
                return;
            }

            _logger.LogInformation("WebSocket connection request from user {UserId}", userId);

            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Store the connection
            _connectedClients.TryAdd(userId, webSocket);
            _logger.LogInformation("WebSocket connection established for user {UserId}", userId);

            try
            {
                // Handle messages until the connection is closed
                await HandleMessages(webSocket, userId);
            }
            finally
            {
                // Clean up the connection
                _connectedClients.TryRemove(userId, out _);
                _logger.LogInformation("WebSocket connection closed for user {UserId}", userId);
            }
        }

        private async Task HandleMessages(WebSocket webSocket, string userId)
        {
            var buffer = new byte[4096];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation("Received message from {UserId}: {Message}", userId, message);

                        // Process message here
                        // For now, we'll just echo it back
                        await SendMessageAsync(webSocket, new { type = "echo", message });
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure,
                            result.CloseStatusDescription, CancellationToken.None);
                        break;
                    }
                }
            }
            catch (WebSocketException ex)
            {
                _logger.LogError(ex, "WebSocket error for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling WebSocket messages for user {UserId}", userId);
            }
            finally
            {
                if (webSocket.State != WebSocketState.Closed)
                {
                    try
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError,
                            "Connection closed due to error", CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error closing WebSocket for user {UserId}", userId);
                    }
                }
            }
        }

        private static async Task SendMessageAsync(WebSocket socket, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        // Helper method to send to a specific user
        public static async Task<bool> SendToUserAsync(string userId, object data)
        {
            if (_connectedClients.TryGetValue(userId, out var socket) &&
                socket.State == WebSocketState.Open)
            {
                try
                {
                    await SendMessageAsync(socket, data);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending to user {userId}: {ex.Message}");
                    return false;
                }
            }
            else
            {
                // Log if client isn't connected or in wrong state
                string state = socket != null ? socket.State.ToString() : "null";
                Console.WriteLine($"Cannot send to user {userId}: Client state is {state}");
                return false;
            }
        }


        // Helper method to broadcast to all connected users
        public static async Task BroadcastAsync(object data)
        {
            foreach (var client in _connectedClients)
            {
                if (client.Value.State == WebSocketState.Open)
                {
                    await SendMessageAsync(client.Value, data);
                }
            }
        }
    }
}
