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

        public async Task<Patient> CreatePatient(Patient patient)
        {
            string query = @"
                INSERT INTO dbo.Patient 
                (Voornaam, Achternaam, Geboortedatum, OuderVoogdID, TrajectID, ArtsID, UserID, AvatarID) 
                VALUES 
                (@Voornaam, @Achternaam, @Geboortedatum, @OuderVoogdID, @TrajectID, @ArtsID, @UserID, @AvatarID);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            // Voer insert uit en haal het nieuwe ID op
            int newId = await _dbConnection.ExecuteScalarAsync<int>(query, patient);

            // Haal de volledige patient record op
            patient.ID = newId;
            return patient;
        }

        public async Task<PatientDto> UpdatePatient(PatientDto patient)
        {
            string query = "UPDATE dbo.Patient SET TrajectID = @TrajectID, ArtsID = @ArtsID WHERE ID = @ID";
            await _dbConnection.ExecuteAsync(query, patient);
            return patient;
        }
    }
}
