using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Services
{
    public class NotificatieBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<NotificatieBackgroundService> _logger;

        public NotificatieBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<NotificatieBackgroundService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificatieBackgroundService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessExpiredNotifications();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing notifications");
                }

                // Check every 10 seconds instead of 1 minute
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        private async Task ProcessExpiredNotifications()
        {
            _logger.LogInformation("Checking for expired notifications");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var notificatieRepository = scope.ServiceProvider.GetRequiredService<INotificatieRepository>();
                var notificatieSender = scope.ServiceProvider.GetRequiredService<INotificatieSender>();

                // Get all unread notifications that have expired
                var expiredNotifications = await notificatieRepository.GetExpiredNotificatiesAsync();
                _logger.LogInformation("Found {Count} expired notifications", expiredNotifications.Count());

                // Get current time for comparison
                var now = DateTime.Now;

                foreach (var notificatie in expiredNotifications)
                {
                    _logger.LogInformation(
                        "Processing notification ID: {NotificatieId}, Expired: {Expired}, IsRead: {IsRead}",
                        notificatie.ID,
                        notificatie.DatumVerloop <= now,
                        notificatie.IsGelezen);

                    // Process if not read and has expired
                    if (!notificatie.IsGelezen && notificatie.DatumVerloop <= now)
                    {
                        _logger.LogInformation(
                            "Sending notification ID: {NotificatieId} to user {UserId}",
                            notificatie.ID,
                            notificatie.UserId);

                        // Send via WebSocket
                        await notificatieSender.SendNotificationAsync(notificatie);

                        // Mark as read
                        notificatie.IsGelezen = true;
                        await notificatieRepository.UpdateNotificatieAsync(notificatie);

                        _logger.LogInformation(
                            "Notification ID: {NotificatieId} marked as read",
                            notificatie.ID);
                    }
                }
            }
        }
    }
}
