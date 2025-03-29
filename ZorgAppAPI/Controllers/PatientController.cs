using Microsoft.AspNetCore.Authorization;
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

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            // Valideer verplichte velden
            if (string.IsNullOrWhiteSpace(patient.Voornaam) ||
                string.IsNullOrWhiteSpace(patient.Achternaam))
            {
                return BadRequest("Voornaam en achternaam zijn verplicht");
            }

            // Controleer geboortedatum
            if (patient.Geboortedatum == default)
            {
                return BadRequest("Geboortedatum is verplicht");
            }

            //try
            //{
            //    // Maak nieuwe patient aan
            //    var createdPatient = await _patientRepository.CreatePatient(patient);

            //    // Retourneer de aangemaakte patient met een 201 Created status
            //    return CreatedAtAction(
            //        nameof(GetPatient),
            //        new { id = createdPatient.ID },
            //        createdPatient
            //    );
            //}
            //catch (Exception ex)
            //{
            //    // Log de exceptie (voeg logging toe indien beschikbaar)
            //    return StatusCode(500, "Er is een fout opgetreden bij het aanmaken van de patient");
            //}
            await _patientRepository.CreatePatient(patient);
            return CreatedAtAction(nameof(GetPatient), new { id = patient.ID }, patient);
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
        public async Task<IActionResult> UpdatePatient([FromBody]PatientDto patient)
        {
            var existingPatient = await _patientRepository.GetPatient(patient.ID);
            if (existingPatient == null)
            {
                return NotFound($"Patient with ID {patient.ID} not found");
            }

            // Update only the ArtsID and TrajectID
            existingPatient.ArtsID = patient.ArtsID;
            existingPatient.TrajectID = patient.TrajectID;

            var updatedPatient = await _patientRepository.UpdatePatient(new PatientDto { ID = patient.ID, ArtsID = patient.ArtsID, TrajectID = patient.TrajectID});
            return Ok(updatedPatient);
        }
    }
}
