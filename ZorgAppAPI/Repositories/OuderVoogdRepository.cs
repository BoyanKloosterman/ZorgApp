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
    }
}
