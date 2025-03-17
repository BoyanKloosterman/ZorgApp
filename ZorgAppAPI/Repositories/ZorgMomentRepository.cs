using Dapper;
using System.Data;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public class ZorgmomentRepository : IZorgmomentRepository
    {
        private readonly IDbConnection _dbConnection;

        public ZorgmomentRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<ZorgMoment> GetZorgmomentByIdAsync(int id)
        {
            var query = "SELECT * FROM dbo.ZorgMoment WHERE ID = @Id";
            return await _dbConnection.QuerySingleOrDefaultAsync<ZorgMoment>(query, new { Id = id });
        }

        public async Task<IEnumerable<ZorgMoment>> GetAllZorgmomentenAsync()
        {
            var query = "SELECT * FROM dbo.ZorgMoment";
            return await _dbConnection.QueryAsync<ZorgMoment>(query);
        }

        public async Task AddZorgmomentAsync(ZorgMoment zorgmoment)
        {
            var query = "INSERT INTO dbo.ZorgMoment (Naam, Url, Plaatje, TijdsDuurInMin) VALUES (@Naam, @Url, @Plaatje, @TijdsDuurInMin)";
            await _dbConnection.ExecuteAsync(query, zorgmoment);
        }

        public async Task UpdateZorgmomentAsync(ZorgMoment zorgmoment)
        {
            var query = "UPDATE dbo.ZorgMoment SET Naam = @Naam, Url = @Url, Plaatje = @Plaatje, TijdsDuurInMin = @TijdsDuurInMin WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, zorgmoment);
        }

        public async Task DeleteZorgmomentAsync(int id)
        {
            var query = "DELETE FROM dbo.ZorgMoment WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id });
        }
    }
}
