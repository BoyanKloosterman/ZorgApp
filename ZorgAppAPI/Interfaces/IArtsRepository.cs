using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IArtsRepository
    {
        Task<IEnumerable<Arts>> GetArts();
        Task<Arts> GetArts(int id);
    }
}
