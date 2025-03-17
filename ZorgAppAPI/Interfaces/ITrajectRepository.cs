using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public interface ITrajectRepository
    {
        Task<Traject> GetTrajectByIdAsync(int id);
        Task<IEnumerable<Traject>> GetAllTrajectsAsync();
        Task AddTrajectAsync(Traject traject);
        Task UpdateTrajectAsync(Traject traject);
        Task DeleteTrajectAsync(int id);
    }
}
