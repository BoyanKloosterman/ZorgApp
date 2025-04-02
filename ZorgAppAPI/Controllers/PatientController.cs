using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IAuthenticationService _authenticationService;

        public PatientController(IPatientRepository patientRepository, IAuthenticationService authenticationService)
        {
            _patientRepository = patientRepository;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _patientRepository.GetPatients();
            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(int id)
        {
            var patient = await _patientRepository.GetPatient(id);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient([FromBody] PatientDto patient)
        {
            var existingPatient = await _patientRepository.GetPatient(patient.ID);
            if (existingPatient == null)
            {
                return NotFound($"Patient with ID {patient.ID} not found");
            }

            // Update only the ArtsID and TrajectID
            existingPatient.ArtsID = patient.ArtsID;
            existingPatient.TrajectID = patient.TrajectID;

            var updatedPatient = await _patientRepository.UpdatePatient(new PatientDto { ID = patient.ID, ArtsID = patient.ArtsID, TrajectID = patient.TrajectID });
            return Ok(updatedPatient);
        }

        [HttpPut("avatar")]
        public async Task<IActionResult> UpdatePatientAvatar([FromBody] PatientAvatarDto patient)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            // Update only the ArtsID and TrajectID

            var updatedPatient = await _patientRepository.UpdatePatientAvatar(new PatientAvatarDto { UserId = userId, AvatarID = patient.AvatarID  });
            return Ok(updatedPatient);
        }

        //get avatar id for user
        [HttpGet("avatar")]
        public async Task<IActionResult> GetPatientAvatar()
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            var patient = await _patientRepository.GetPatientAvatar(userId);
            if (patient == null)
            {
                return NotFound();
            }
            return Ok(patient);
        }
    }
}
