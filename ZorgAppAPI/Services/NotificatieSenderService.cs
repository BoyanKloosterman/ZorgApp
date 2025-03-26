using System.Threading.Tasks;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using Microsoft.Extensions.Logging;

namespace ZorgAppAPI.Services
{
    public class NotificatieSender : INotificatieSender
    {
        private readonly ILogger<NotificatieSender> _logger;

        public NotificatieSender(ILogger<NotificatieSender> logger)
        {
            _logger = logger;
        }

        public Task SendNotificationAsync(Notificatie notificatie)
        {
            // Implement the logic to send the notification (e.g., email, SMS, etc.)
            _logger.LogInformation("Notification sent for Notificatie ID: {NotificatieId}", notificatie.ID);
            return Task.CompletedTask;
        }
    }
}
