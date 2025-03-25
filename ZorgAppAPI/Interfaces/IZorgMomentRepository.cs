using System.Collections.Generic;
using System.Threading.Tasks;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public interface IZorgmomentRepository
    {
        Task<ZorgMoment> GetZorgmomentByIdAsync(int id);
        Task<IEnumerable<int>> GetAllZorgmomenten(string userId);
    }
}