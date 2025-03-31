using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IUserZorgMomentRepository
    {
        Task<UserZorgMoment> AddUserZorgMomentAsync(string userId, int zorgMomentId);
        Task<IEnumerable<int>> GetUserZorgMomentsByUserIdAsync(string userId); 
    }
}
