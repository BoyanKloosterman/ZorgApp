using Dapper;
using System.Data;
using ZorgAppAPI.Models;
using ZorgAppAPI.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;

namespace ZorgAppAPI.Repositories
{
    public class NotificatieRepository : INotificatieRepository
    {
        private readonly string _connectionString;

        public NotificatieRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<Notificatie> GetNotificatieByIdAsync(int id)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM dbo.Notificatie WHERE ID = @Id";
                return await dbConnection.QuerySingleOrDefaultAsync<Notificatie>(query, new { Id = id });
            }
        }

        public async Task<IEnumerable<Notificatie>> GetAllNotificatiesAsync()
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM dbo.Notificatie";
                return await dbConnection.QueryAsync<Notificatie>(query);
            }
        }

        public async Task AddNotificatieAsync(Notificatie notificatie)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO dbo.Notificatie (Bericht, IsGelezen, DatumAanmaak, DatumVerloop, UserId) VALUES (@Bericht, @IsGelezen, @DatumAanmaak, @DatumVerloop, @UserId)";
                await dbConnection.ExecuteAsync(query, notificatie);
            }
        }

        public async Task UpdateNotificatieAsync(Notificatie notificatie)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var query = "UPDATE dbo.Notificatie SET Bericht = @Bericht, IsGelezen = @IsGelezen, DatumAanmaak = @DatumAanmaak, DatumVerloop = @DatumVerloop, UserId = @UserId WHERE ID = @Id";
                await dbConnection.ExecuteAsync(query, notificatie);
            }
        }

        public async Task DeleteNotificatieAsync(int id)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var query = "DELETE FROM dbo.Notificatie WHERE ID = @Id";
                await dbConnection.ExecuteAsync(query, new { Id = id });
            }
        }

        public async Task<IEnumerable<Notificatie>> GetNotificatiesByUserIdAsync(string userId)
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM dbo.Notificatie WHERE UserId = @UserId";
                return await dbConnection.QueryAsync<Notificatie>(query, new { UserId = userId });
            }
        }

        public async Task<IEnumerable<Notificatie>> GetExpiredNotificatiesAsync()
        {
            using (IDbConnection dbConnection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM dbo.Notificatie WHERE DatumVerloop <= GETDATE() AND IsGelezen = 0";
                return await dbConnection.QueryAsync<Notificatie>(query);
            }
        }
    }
}
