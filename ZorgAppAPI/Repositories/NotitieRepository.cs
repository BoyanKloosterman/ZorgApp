using Dapper;
using System.Data;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public class NotitieRepository : INotitieRepository
    {
        private readonly IDbConnection _dbConnection;

        public NotitieRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Notitie> GetNotitieByIdAsync(int id)
        {
            var query = "SELECT * FROM dbo.Notitie WHERE ID = @Id";
            return await _dbConnection.QuerySingleOrDefaultAsync<Notitie>(query, new { Id = id });
        }

        public async Task<IEnumerable<Notitie>> GetAllNotitiesAsync()
        {
            var query = "SELECT * FROM dbo.Notitie";
            return await _dbConnection.QueryAsync<Notitie>(query);
        }

        public async Task AddNotitieAsync(Notitie notitie)
        {
            var query = "INSERT INTO dbo.Notitie (Titel, Tekst, UserId, DatumAanmaak) VALUES (@Titel, @Tekst, @UserId, @DatumAanmaak)";
            await _dbConnection.ExecuteAsync(query, notitie);

        }

        public async Task UpdateNotitieAsync(Notitie notitie)
        {
            var query = "UPDATE dbo.Notitie SET Titel = @Titel, Tekst = @Tekst, UserId = @UserId, DatumAanmaak = @DatumAanmaak WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, notitie);
        }

        public async Task DeleteNotitieAsync(int id)
        {
            var query = "DELETE FROM dbo.Notitie WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id });
        }
    }
}
