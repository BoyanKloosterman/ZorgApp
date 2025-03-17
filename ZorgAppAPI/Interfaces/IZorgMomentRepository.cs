using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public interface IZorgmomentRepository
    {
        Task<ZorgMoment> GetZorgmomentByIdAsync(int id);
        Task<IEnumerable<ZorgMoment>> GetAllZorgmomentenAsync();
        Task AddZorgmomentAsync(ZorgMoment zorgmoment);
        Task UpdateZorgmomentAsync(ZorgMoment zorgmoment);
        Task DeleteZorgmomentAsync(int id);
    }
}
