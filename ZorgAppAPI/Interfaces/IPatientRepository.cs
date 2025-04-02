using ZorgAppAPI.Models;

namespace ZorgAppAPI.Interfaces
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetPatients();
        Task<Patient> GetPatient(int id);
        Task<PatientDto> UpdatePatient(PatientDto patient);
        Task<Patient> CreatePatient(Patient patient);
        Task<PatientAvatarDto> UpdatePatientAvatar(PatientAvatarDto patient);
        Task<PatientAvatarDto> GetPatientAvatar(string userId);
    }
}
