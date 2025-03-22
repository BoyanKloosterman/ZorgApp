using Dapper;
using System.Data;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly IDbConnection _dbConnection;

        public PatientRepository(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        public async Task<IEnumerable<Patient>> GetPatients()
        {
            string query = "SELECT * FROM dbo.Patient";
            return await _dbConnection.QueryAsync<Patient>(query);
        }
        public async Task<Patient> GetPatient(int id)
        {
            string query = "SELECT * FROM dbo.Patient WHERE ID = @Id";
            return await _dbConnection.QuerySingleOrDefaultAsync<Patient>(query, new { Id = id });
        }
    }
}
