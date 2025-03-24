using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetPatients();
        Task<Patient> GetPatient(int id);
        Task<Patient> UpdatePatient(Patient patient);
    }
}
