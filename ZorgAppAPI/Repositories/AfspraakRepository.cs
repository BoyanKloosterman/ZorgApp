using Dapper;
using System.Data;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public class AfspraakRepository : IAfspraakRepository
    {
        private readonly IDbConnection _dbConnection;

        public AfspraakRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Afspraak>> GetAllAsync()
        {
            var query = "SELECT * FROM dbo.Afspraak";
            return await _dbConnection.QueryAsync<Afspraak>(query);
        }

        public async Task<Afspraak?> GetByIdAsync(int id)
        {
            var query = "SELECT * FROM dbo.Afspraak WHERE ID = @Id";
            return await _dbConnection.QuerySingleOrDefaultAsync<Afspraak>(query, new { Id = id });
        }

        public async Task AddAsync(Afspraak afspraak)
        {
            var query = "INSERT INTO dbo.Afspraak (UserId, ArtsID, Datumtijd, Naam) VALUES (@UserId, @ArtsID, @Datumtijd, @Naam)";
            await _dbConnection.ExecuteAsync(query, afspraak);
        }

        public async Task UpdateAsync(Afspraak afspraak, string userId, bool isArts)
        {
            var existingAfspraak = await GetByIdAsync(afspraak.ID);
            if (existingAfspraak == null)
                throw new InvalidOperationException("Afspraak niet gevonden.");

            if (!isArts && existingAfspraak.UserId != userId)
                throw new UnauthorizedAccessException("Geen toestemming om deze afspraak te wijzigen.");

            var query = "UPDATE dbo.Afspraak SET UserId = @UserId, ArtsID = @ArtsID, Datumtijd = @Datumtijd, Naam = @Naam WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, afspraak);
        }

        public async Task DeleteAsync(int id, string userId, bool isArts)
        {
            var afspraak = await GetByIdAsync(id);
            if (afspraak == null)
                throw new InvalidOperationException("Afspraak niet gevonden.");

            if (!isArts && afspraak.UserId != userId)
                throw new UnauthorizedAccessException("Geen toestemming om deze afspraak te verwijderen.");

            var query = "DELETE FROM dbo.Afspraak WHERE ID = @Id";
            await _dbConnection.ExecuteAsync(query, new { Id = id });
        }

        public async Task<IEnumerable<Afspraak>> GetByUserIdAsync(string userId)
        {
            var query = "SELECT * FROM dbo.Afspraak WHERE UserId = @UserId";
            return await _dbConnection.QueryAsync<Afspraak>(query, new { UserId = userId });
        }
    }
}
