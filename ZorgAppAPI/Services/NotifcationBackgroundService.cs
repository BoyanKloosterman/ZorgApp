using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using ZorgAppAPI.Interfaces;

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
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("NotificatieBackgroundService is running.");

                try
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var notificatieRepository = scope.ServiceProvider.GetRequiredService<INotificatieRepository>();
                        var notificatieSender = scope.ServiceProvider.GetRequiredService<INotificatieSender>();

                        var notificaties = await notificatieRepository.GetExpiredNotificatiesAsync();
                        foreach (var notificatie in notificaties)
                        {
                            _logger.LogInformation("Sending notification for Notificatie ID: {NotificatieId}", notificatie.ID);
                            await notificatieSender.SendNotificationAsync(notificatie);
                            notificatie.IsGelezen = true;
                            await notificatieRepository.UpdateNotificatieAsync(notificatie);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while processing notifications.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
