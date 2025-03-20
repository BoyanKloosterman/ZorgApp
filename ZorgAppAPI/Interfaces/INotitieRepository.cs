using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public interface INotitieRepository
    {
        Task<Notitie> GetNotitieByIdAsync(int id);
        Task<IEnumerable<Notitie>> GetAllNotitiesAsync();
        Task AddNotitieAsync(Notitie notitie);
        Task UpdateNotitieAsync(Notitie notitie);
        Task DeleteNotitieAsync(int id);
    }
}
