﻿using Dapper;
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

        public async Task<UserZorgMoment> AddUserZorgMomentAsync(string userId, int zorgMomentId)
        {
            var query = @"INSERT INTO dbo.User_Zorgmoment (UserId, ZorgmomentId)
                OUTPUT INSERTED.UserId, INSERTED.ZorgmomentId, INSERTED.CreatedAt
                VALUES (@UserId, @ZorgmomentId)";

            var parameters = new
            {
                UserId = userId,
                ZorgmomentId = zorgMomentId
            };

            return await _dbConnection.QuerySingleAsync<UserZorgMoment>(query, parameters);
        }

        public async Task<IEnumerable<int>> GetUserZorgMomentsByUserIdAsync(string userId)
        {
            var query = "SELECT ZorgmomentId FROM dbo.User_Zorgmoment WHERE UserId = @UserId";
            return await _dbConnection.QueryAsync<int>(query, new { UserId = userId });
        }

    }
}