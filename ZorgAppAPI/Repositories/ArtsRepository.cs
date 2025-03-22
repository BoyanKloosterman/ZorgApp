using Dapper;
using System.Data;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public class ArtsRepository : IArtsRepository
    {
        private readonly IDbConnection _dbConnection;

        public ArtsRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Arts>> GetArts()
        {
           var query = "SELECT * FROM dbo.Arts";
           return await _dbConnection.QueryAsync<Arts>(query);
        }
        public async Task<Arts> GetArts(int id)
        {
            var query = "SELECT * FROM dbo.Arts WHERE ID = @ID";
            return await _dbConnection.QueryFirstOrDefaultAsync<Arts>(query, new { ID = id });
        }

    }
}
