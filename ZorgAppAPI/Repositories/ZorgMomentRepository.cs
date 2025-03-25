using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<IEnumerable<int>> GetAllZorgmomenten(string userId)
        {
            // 1. Haal TrajectID op uit Patient tabel
            var trajectId = await _dbConnection.QueryFirstOrDefaultAsync<int?>(
                "SELECT TrajectID FROM dbo.Patient WHERE UserId = @UserId",
                new { UserId = userId });

            if (!trajectId.HasValue)
            {
                Console.WriteLine("Geen traject gevonden voor gebruiker");
                return new List<int>();
            }

            // 2. Haal geordende ZorgMoment IDs op
            var zorgMomentIds = (await _dbConnection.QueryAsync<int>(
                @"SELECT ZorgMomentId 
                FROM dbo.Traject_ZorgMoment 
                WHERE TrajectId = @TrajectId
                ORDER BY Volgorde",
                new { TrajectId = trajectId.Value })).ToList();

            Console.WriteLine($"Gevonden IDs: {string.Join(", ", zorgMomentIds)}");

            return zorgMomentIds; // Retourneer direct de IDs
        }
    }



        //public async Task AddZorgmomentAsync(ZorgMoment zorgmoment)
        //{
        //    var query = "INSERT INTO dbo.ZorgMoment (Naam, Url, Plaatje, TijdsDuurInMin) VALUES (@Naam, @Url, @Plaatje, @TijdsDuurInMin)";
        //    await _dbConnection.ExecuteAsync(query, zorgmoment);
        //}

        //public async Task UpdateZorgmomentAsync(ZorgMoment zorgmoment)
        //{
        //    var query = "UPDATE dbo.ZorgMoment SET Naam = @Naam, Url = @Url, Plaatje = @Plaatje, TijdsDuurInMin = @TijdsDuurInMin WHERE ID = @Id";
        //    await _dbConnection.ExecuteAsync(query, zorgmoment);
        //}

        //public async Task DeleteZorgmomentAsync(int id)
        //{
        //    var query = "DELETE FROM dbo.ZorgMoment WHERE ID = @Id";
        //    await _dbConnection.ExecuteAsync(query, new { Id = id });
        //}
}
