using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface INotificatieRepository
    {
        Task<Notificatie> GetNotificatieByIdAsync(int id);
        Task<IEnumerable<Notificatie>> GetAllNotificatiesAsync();
        Task AddNotificatieAsync(Notificatie notificatie);
        Task UpdateNotificatieAsync(Notificatie notificatie);
        Task DeleteNotificatieAsync(int id);
        Task<IEnumerable<Notificatie>> GetNotificatiesByUserIdAsync(string userId);
    }
}
