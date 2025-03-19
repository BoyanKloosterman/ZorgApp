namespace ZorgAppAPI.Interfaces
{
    public interface IIdentityRepository
    {
        Task<(bool Succeeded, string UserId, IEnumerable<string> Errors)> CreateUserAsync(string email, string password);
        Task<bool> AddToRoleAsync(string userId, string role);
    }
}
