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
            _logger.LogInformation(
                "Sending notification ID: {NotificatieId}, Message: {Message}, UserId: {UserId}",
                notificatie.ID,
                notificatie.Bericht,
                notificatie.UserId);

            try
            {
                // Create notification data object with all important fields
                var notificationData = new
                {
                    type = "notification",
                    id = notificatie.ID,
                    message = notificatie.Bericht,
                    created = notificatie.DatumAanmaak.ToString("yyyy-MM-dd HH:mm:ss"),
                    expired = notificatie.DatumVerloop.ToString("yyyy-MM-dd HH:mm:ss"),
                    isExpired = notificatie.DatumVerloop <= DateTime.Now
                };

                // Direct attempt to the specific user
                if (!string.IsNullOrEmpty(notificatie.UserId))
                {
                    _logger.LogInformation("Attempting WebSocket notification to user {UserId}", notificatie.UserId);
                    bool delivered = await WebSocketController.SendToUserAsync(notificatie.UserId, notificationData);

                    if (delivered)
                    {
                        _logger.LogInformation("Successfully delivered notification to user {UserId}", notificatie.UserId);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "User {UserId} not connected via WebSocket, notification not delivered directly",
                            notificatie.UserId);

                        // Try broadcasting as a fallback
                        _logger.LogInformation("Broadcasting notification as fallback");
                        await WebSocketController.BroadcastAsync(notificationData);
                    }
                }
                else
                {
                    // Broadcast to all users if no specific user
                    _logger.LogInformation("Broadcasting notification to all users");
                    await WebSocketController.BroadcastAsync(notificationData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending WebSocket notification");
            }
        }
    }
}
