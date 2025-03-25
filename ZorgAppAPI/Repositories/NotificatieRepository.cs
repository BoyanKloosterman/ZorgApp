using Dapper;
using System.Data;
using ZorgAppAPI.Models;
using ZorgAppAPI.Interfaces;

namespace ZorgAppAPI.Repositories
{
    public class NotificatieRepository : INotificatieRepository
    {
        private readonly IDbConnection _dbConnection;

        public NotificatieRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<Notificatie> GetNotificatieByIdAsync(int id)
        {
            var query = "SELECT * FROM dbo.Notificatie WHERE ID = @Id";
            return await _dbConnection.QuerySingleOrDefaultAsync<Notificatie>(query, new { Id = id });
        }

        public async Task<IEnumerable<Notificatie>> GetAllNotificatiesAsync()
        {
            var query = "SELECT * FROM dbo.Notificatie";
            return await _dbConnection.QueryAsync<Notificatie>(query);
        }

        public async Task AddNotificatieAsync(Notificatie notificatie)
        {
            var query = "INSERT INTO dbo.Notificatie (Bericht, IsGelezen, DatumAanmaak, DatumVerloop, UserId) VALUES (@Bericht, @IsGelezen, @DatumAanmaak, @DatumVerloop, @UserId)";
            await _dbConnection.ExecuteAsync(query, notificatie);
        }

        public async Task UpdateNotificatieAsync(Notificatie notificatie)
        {
            var query = "UPDATE dbo.Notificatie SET Bericht = @Bericht, IsGelezen = @IsGelezen, DatumAanmaak = @DatumAanmaak, DatumVerloop = @DatumVerloop, UserId = @UserId WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, notificatie);
        }

        public async Task DeleteNotificatieAsync(int id)
        {
            var query = "DELETE FROM dbo.Notificatie WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<Notificatie>> GetNotificatiesByUserIdAsync(string userId)
        {
            var query = "SELECT * FROM dbo.Notificatie WHERE UserId = @UserId";
            return await _dbConnection.QueryAsync<Notificatie>(query, new { UserId = userId });
        }

        public async Task<IEnumerable<Notificatie>> GetExpiredNotificatiesAsync()
        {
            var query = "SELECT * FROM dbo.Notificatie WHERE DatumVerloop <= GETDATE() AND IsGelezen = 0";
            return await _dbConnection.QueryAsync<Notificatie>(query);
        }
    }
}
