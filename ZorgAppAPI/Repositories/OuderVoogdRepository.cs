using Dapper;
using System.Data;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public class OuderVoogdRepository : IOuderVoogdRepository
    {
        private readonly IDbConnection _dbConnection;
        public OuderVoogdRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }
        public async Task<IEnumerable<OuderVoogd>> GetOuderVoogden()
        {
            var query = "SELECT * FROM dbo.OuderVoogd";
            return await _dbConnection.QueryAsync<OuderVoogd>(query);
        }
        public async Task<OuderVoogd> GetOuderVoogd(int id)
        {
            var query = "SELECT * FROM dbo.OuderVoogd WHERE ID = @ID";
            return await _dbConnection.QueryFirstOrDefaultAsync<OuderVoogd>(query, new { ID = id });
        }
        public async Task<OuderVoogd> GetCurrentOuderVoogd()
        {
            // In een echte applicatie zou je hier de huidige ingelogde gebruiker ophalen
            // Voor nu nemen we een standaard id of de eerste beschikbare ouder/voogd
            var query = "SELECT TOP 1 * FROM dbo.OuderVoogd";
            return await _dbConnection.QueryFirstOrDefaultAsync<OuderVoogd>(query);
        }
    }
}
