// Repositories/UserProfileRepository.cs
using Dapper;
using System.Data;
using ZorgAppAPI.Interfaces;

namespace ZorgAppAPI.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly IDbConnection _dbConnection;

        public UserProfileRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<int> CreateArtsAsync(string userId, string voornaam, string achternaam)
        {
            const string sql = @"
                INSERT INTO [dbo].[Arts] (Voornaam, Achternaam, UserId)
                VALUES (@Voornaam, @Achternaam, @UserId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            return await _dbConnection.QuerySingleAsync<int>(sql, new { Voornaam = voornaam, Achternaam = achternaam, UserId = userId });
        }

        public async Task<int> CreatePatientAsync(string userId, string voornaam, string achternaam, DateTime geboortedatum)
        {
            const string sql = @"
                INSERT INTO [dbo].[Patient] (Voornaam, Achternaam, Geboortedatum, UserId)
                VALUES (@Voornaam, @Achternaam, @Geboortedatum, @UserId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            return await _dbConnection.QuerySingleAsync<int>(sql,
                new { Voornaam = voornaam, Achternaam = achternaam, Geboortedatum = geboortedatum, UserId = userId });
        }

        public async Task<int> CreateOuderVoogdAsync(string userId, string voornaam, string achternaam)
        {
            const string sql = @"
                INSERT INTO [dbo].[OuderVoogd] (Voornaam, Achternaam, UserId)
                VALUES (@Voornaam, @Achternaam, @UserId);
                SELECT CAST(SCOPE_IDENTITY() as int)";

            return await _dbConnection.QuerySingleAsync<int>(sql, new { Voornaam = voornaam, Achternaam = achternaam, UserId = userId });
        }

        public async Task<string> GetRoleIdByNameAsync(string roleName)
        {
            const string sql = "SELECT Id FROM [auth].[AspNetRoles] WHERE [Name] = @RoleName";
            return await _dbConnection.QuerySingleOrDefaultAsync<string>(sql, new { RoleName = roleName });
        }

        public async Task AssignRoleToUserAsync(string userId, string roleId)
        {
            const string sql = @"
                INSERT INTO [auth].[AspNetUserRoles] (UserId, RoleId)
                VALUES (@UserId, @RoleId)";

            await _dbConnection.ExecuteAsync(sql, new { UserId = userId, RoleId = roleId });
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            const string sql = @"
                SELECT COUNT(1) 
                FROM [auth].[AspNetUsers] 
                WHERE NormalizedEmail = @NormalizedEmail";

            var count = await _dbConnection.ExecuteScalarAsync<int>(sql,
                new { NormalizedEmail = email.ToUpperInvariant() });
            return count > 0;
        }
    }
}

