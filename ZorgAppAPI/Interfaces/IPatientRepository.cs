using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetPatients();
        Task<Patient> GetPatient(int id);
        Task<PatientDto> UpdatePatient(PatientDto patient);
    }
}
