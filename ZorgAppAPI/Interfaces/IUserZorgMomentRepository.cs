using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IUserZorgMomentRepository
    {
        Task<UserZorgMoment> AddUserZorgMomentAsync(UserZorgMoment userCheckpoint);
        Task<IEnumerable<UserZorgMoment>> GetUserZorgMomentsByUserIdAsync(string userId);
    }
}
