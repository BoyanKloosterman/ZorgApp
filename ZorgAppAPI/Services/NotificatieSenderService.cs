using System.Threading.Tasks;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using Microsoft.Extensions.Logging;
using ZorgAppAPI.Controllers;

namespace ZorgAppAPI.Services
{
    public class NotificatieSender : INotificatieSender
    {
        private readonly ILogger<NotificatieSender> _logger;

        public NotificatieSender(ILogger<NotificatieSender> logger)
        {
            _logger = logger;
        }

        public async Task SendNotificationAsync(Notificatie notificatie)
        {
            _logger.LogInformation("Sending notification ID: {NotificatieId}, Message: {Message}, UserId: {UserId}",
                notificatie.ID, notificatie.Bericht, notificatie.UserId);

            try
            {
                // Create notification data object
                var notificationData = new
                {
                    type = "notification",
                    id = notificatie.ID,
                    message = notificatie.Bericht,
                    created = notificatie.DatumAanmaak.ToString("yyyy-MM-dd HH:mm:ss"),
                    expired = notificatie.DatumVerloop.ToString("yyyy-MM-dd HH:mm:ss")
                };

                bool delivered = false;

                if (!string.IsNullOrEmpty(notificatie.UserId))
                {
                    // Try to send to the specific user
                    delivered = await WebSocketController.SendToUserAsync(notificatie.UserId, notificationData);

                    if (delivered)
                    {
                        _logger.LogInformation("Successfully delivered notification to user {UserId}", notificatie.UserId);
                    }
                    else
                    {
                        _logger.LogWarning("User {UserId} not connected, trying broadcast instead", notificatie.UserId);
                        // If specific user not connected, try broadcasting as fallback
                        await WebSocketController.BroadcastAsync(notificationData);
                    }
                }
                else
                {
                    // Broadcast to all if no specific user
                    _logger.LogInformation("Broadcasting notification to all connected users");
                    await WebSocketController.BroadcastAsync(notificationData);
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error sending WebSocket notification");
            }
        }
    }
}
