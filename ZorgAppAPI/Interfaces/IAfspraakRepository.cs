using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IAfspraakRepository
    {
        Task<IEnumerable<Afspraak>> GetAllAsync();
        Task<Afspraak?> GetByIdAsync(int id);
        Task AddAsync(Afspraak afspraak);
        Task UpdateAsync(Afspraak afspraak, string userId, bool isArts);
        Task DeleteAsync(int id, string userId, bool isArts);
    }
}
