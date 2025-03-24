using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        public PatientController(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
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
        public async Task<IActionResult> UpdatePatient(int id, [FromBody]Patient patient)
        {
            var existingPatient = await _patientRepository.GetPatient(id);
            if (existingPatient == null)
            {
                return NotFound($"Patient with ID {id} not found");
            }

            // Update only the ArtsID and TrajectID
            existingPatient.ArtsID = patient.ArtsID;
            existingPatient.TrajectID = patient.TrajectID;

            var updatedPatient = await _patientRepository.UpdatePatient(existingPatient);
            return Ok(updatedPatient);
        }
    }
}
