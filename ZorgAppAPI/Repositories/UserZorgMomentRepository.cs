using Dapper;
using System.Data;
using ZorgAppAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZorgAppAPI.Interfaces;

namespace ZorgAppAPI.Repositories
{
    public class UserZorgMomentRepository : IUserZorgMomentRepository
    {
        private readonly IDbConnection _dbConnection;

        public UserZorgMomentRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<UserZorgMoment> AddUserZorgMomentAsync(UserZorgMoment userZorgMoment)
        {
            var query = @"INSERT INTO dbo.User_ZorgMoment (UserId, ZorgMomentId)
                        OUTPUT INSERTED.*
                        VALUES (@UserId, @ZorgMomentId)";

            return await _dbConnection.QuerySingleAsync<UserZorgMoment>(query, userZorgMoment);
        }

        public async Task<IEnumerable<UserZorgMoment>> GetUserZorgMomentsByUserIdAsync(string userId)
        {
            var query = "SELECT * FROM dbo.User_ZorgMoment WHERE UserId = @UserId";
            return await _dbConnection.QueryAsync<UserZorgMoment>(query, new { UserId = userId });
        }

    }
}