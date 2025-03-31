using System.Threading.Tasks;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface INotificatieSender
    {
        Task SendNotificationAsync(Notificatie notificatie);
    }
}