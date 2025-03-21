namespace ZorgAppAPI.Interfaces
{
    public interface IUserProfileRepository
    {
        Task<int> CreateArtsAsync(string userId, string voornaam, string achternaam);
        Task<int> CreatePatientAsync(string userId, string voornaam, string achternaam, DateTime geboortedatum);
        Task<int> CreateOuderVoogdAsync(string userId, string voornaam, string achternaam);
        Task<string> GetRoleIdByNameAsync(string roleName);
        Task AssignRoleToUserAsync(string userId, string roleId);
        Task<bool> EmailExistsAsync(string email);
    }
}