using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Data.SqlClient;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityRepository _identityRepository;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly ILogger<AuthController> _logger;
        private readonly SqlConnection _db;

        public AuthController(
            IIdentityRepository identityRepository,
            IUserProfileRepository userProfileRepository,
            ILogger<AuthController> logger,
            IDbConnection db)
        {
            _identityRepository = identityRepository;
            _userProfileRepository = userProfileRepository;
            _logger = logger;
            _db = (SqlConnection)db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
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
                if (!await _identityRepository.AddToRoleAsync(userId, model.Role))
                {
                    _logger.LogError("Failed to assign role for user {UserId}", userId);
                    return StatusCode(500, "Failed to assign role");
                }

                // Create profile based on role
                int profileId;
                try
                {
                    switch (model.Role)
                    {
                        case "Arts":
                            using (var command = new SqlCommand())
                            {
                                command.Connection = _db;
                                command.Transaction = transaction;
                                command.CommandText = "INSERT INTO dbo.Arts (Voornaam, Achternaam, UserId) VALUES (@Voornaam, @Achternaam, @UserId); SELECT SCOPE_IDENTITY();";

                                command.Parameters.AddWithValue("@Voornaam", model.Voornaam);
                                command.Parameters.AddWithValue("@Achternaam", model.Achternaam);
                                command.Parameters.AddWithValue("@UserId", userId);

                                var result = await command.ExecuteScalarAsync();
                                profileId = Convert.ToInt32(result);
                            }
                            break;

                        case "OuderVoogd":
                            using (var command = new SqlCommand())
                            {
                                command.Connection = _db;
                                command.Transaction = transaction;
                                command.CommandText = "INSERT INTO dbo.OuderVoogd (Voornaam, Achternaam, UserId) VALUES (@Voornaam, @Achternaam, @UserId); SELECT SCOPE_IDENTITY();";

                                command.Parameters.AddWithValue("@Voornaam", model.Voornaam);
                                command.Parameters.AddWithValue("@Achternaam", model.Achternaam);
                                command.Parameters.AddWithValue("@UserId", userId);

                                var result = await command.ExecuteScalarAsync();
                                profileId = Convert.ToInt32(result);
                            }
                            break;

                        case "Patient":
                            using (var command = new SqlCommand())
                            {
                                command.Connection = _db;
                                command.Transaction = transaction;
                                command.CommandText = "INSERT INTO dbo.Patient (Voornaam, Achternaam, UserId, GeboorteDatum) VALUES (@Voornaam, @Achternaam, @UserId, @GeboorteDatum); SELECT SCOPE_IDENTITY();";

                                command.Parameters.AddWithValue("@Voornaam", model.Voornaam);
                                command.Parameters.AddWithValue("@Achternaam", model.Achternaam);
                                command.Parameters.AddWithValue("@UserId", userId);

                                var result = await command.ExecuteScalarAsync();
                                profileId = Convert.ToInt32(result);
                            }
                            break;


                        default:
                            return BadRequest("Invalid role specified");
                    }
                }
                catch (SqlException sqlEx)
                {
                    _logger.LogError(sqlEx, "SQL error creating profile for user {UserId}", userId);
                    await transaction.RollbackAsync();
                    return StatusCode(500, "A database error occurred during registration");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating profile for user {UserId}", userId);
                    await transaction.RollbackAsync();
                    return StatusCode(500, "An error occurred during registration");
                }

                await transaction.CommitAsync();
                return Ok(new { message = "Registration successful", userId, profileId });
            }
            catch (Exception ex)
            {
                if (transaction != null)
                    await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during registration");
                return StatusCode(500, "An error occurred during registration");
            }
            finally
            {
                if (_db.State == ConnectionState.Open)
                    await _db.CloseAsync();
            }
        }
    }
}
