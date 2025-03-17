using Dapper;
using System.Data;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public class TrajectRepository : ITrajectRepository
    {
        private readonly IDbConnection _dbConnection;

        public TrajectRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Traject> GetTrajectByIdAsync(int id)
        {
            var query = "SELECT * FROM dbo.Traject WHERE ID = @Id";
            return await _dbConnection.QuerySingleOrDefaultAsync<Traject>(query, new { Id = id });
        }

        public async Task<IEnumerable<Traject>> GetAllTrajectsAsync()
        {
            var query = "SELECT * FROM dbo.Traject";
            return await _dbConnection.QueryAsync<Traject>(query);
        }

        public async Task AddTrajectAsync(Traject traject)
        {
            var query = "INSERT INTO dbo.Traject (Naam, Omschrijving) VALUES (@Naam, @Omschrijving)";
            await _dbConnection.ExecuteAsync(query, traject);
        }

        public async Task UpdateTrajectAsync(Traject traject)
        {
            var query = "UPDATE dbo.Traject SET Naam = @Naam, Omschrijving = @Omschrijving WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, traject);
        }

        public async Task DeleteTrajectAsync(int id)
        {
            var query = "DELETE FROM dbo.Traject WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id });
        }
    }
}
