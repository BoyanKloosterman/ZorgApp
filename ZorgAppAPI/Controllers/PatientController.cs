using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;
using Microsoft.AspNetCore.Authorization;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly ILogger<PatientController> _logger;
        private readonly SqlConnection _db;
        private readonly IAuthenticationService _authenticationService;
        private readonly IIdentityRepository _identityRepository;

        public PatientController(
            IAuthenticationService authenticationService,
            IIdentityRepository identityRepository,
            IPatientRepository patientRepository,
            ILogger<PatientController> logger,
            IDbConnection db)
        {
            _authenticationService = authenticationService;
            _identityRepository = identityRepository;
            _patientRepository = patientRepository;
            _logger = logger;
            _db = (SqlConnection)db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _patientRepository.GetPatients();
            return Ok(patients);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterPatient([FromBody] PatientRegisterDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            SqlTransaction? transaction = null;
            try
            {
                if (_db.State != ConnectionState.Open)
                    await _db.OpenAsync();

                transaction = (SqlTransaction)await _db.BeginTransactionAsync();

                // Create user with Identity
                var (succeeded, userId, errors) = await _identityRepository.CreateUserAsync(model.Email, model.Password);

                if (!succeeded)
                {
                    _logger.LogError("User creation failed: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { Errors = errors });
                }

                // Add user to role
                if (!await _identityRepository.AddToRoleAsync(userId, "Patient"))
                {
                    _logger.LogError("Failed to assign Patient role for user {UserId}", userId);
                    return StatusCode(500, "Failed to assign role");
                }

                // Create patient profile
                int patientId;
                try
                {
                    using (var command = new SqlCommand())
                    {
                        command.Connection = _db;
                        command.Transaction = transaction;
                        command.CommandText = @"
                            INSERT INTO dbo.Patient (
                                Voornaam, 
                                Achternaam, 
                                UserId, 
                                GeboorteDatum, 
                                AvatarID,
                                OuderVoogdId,
                                TrajectId,
                                ArtsId
                            ) 
                            VALUES (
                                @Voornaam, 
                                @Achternaam, 
                                @UserId, 
                                @GeboorteDatum, 
                                @AvatarID,
                                @OuderVoogdId,
                                @TrajectId,
                                @ArtsId
                            ); 
                            SELECT SCOPE_IDENTITY();";

                        command.Parameters.AddWithValue("@Voornaam", model.Voornaam);
                        command.Parameters.AddWithValue("@Achternaam", model.Achternaam);
                        command.Parameters.AddWithValue("@UserId", userId);
                        command.Parameters.AddWithValue("@GeboorteDatum", model.GeboorteDatum);
                        command.Parameters.AddWithValue("@AvatarID", model.AvatarId);

                        // Handle nullable parameters
                        if (model.OuderVoogdId.HasValue)
                            command.Parameters.AddWithValue("@OuderVoogdId", model.OuderVoogdId.Value);
                        else
                            command.Parameters.AddWithValue("@OuderVoogdId", DBNull.Value);

                        if (model.TrajectId.HasValue)
                            command.Parameters.AddWithValue("@TrajectId", model.TrajectId.Value);
                        else
                            command.Parameters.AddWithValue("@TrajectId", DBNull.Value);

                        if (model.ArtsId.HasValue)
                            command.Parameters.AddWithValue("@ArtsId", model.ArtsId.Value);
                        else
                            command.Parameters.AddWithValue("@ArtsId", DBNull.Value);

                        var result = await command.ExecuteScalarAsync();
                        patientId = Convert.ToInt32(result);
                    }
                }
                catch (SqlException sqlEx)
                {
                    _logger.LogError(sqlEx, "SQL error creating patient profile for user {UserId}", userId);
                    await transaction.RollbackAsync();
                    return StatusCode(500, "A database error occurred during patient registration");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating patient profile for user {UserId}", userId);
                    await transaction.RollbackAsync();
                    return StatusCode(500, "An error occurred during patient registration");
                }

                await transaction.CommitAsync();

                // Return patient ID with success message
                return Ok(new
                {
                    message = "Patient registration successful",
                    userId,
                    patientId
                });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during patient registration");
                return StatusCode(500, "An error occurred during patient registration");
            }
            finally
            {
                if (_db.State == ConnectionState.Open)
                    await _db.CloseAsync();
            }
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
