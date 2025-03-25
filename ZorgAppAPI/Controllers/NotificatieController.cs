using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificatieController : ControllerBase
    {
        private readonly INotificatieRepository _notificatieRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<NotificatieController> _logger;

        public NotificatieController(
            INotificatieRepository notificatieRepository,
            IAuthenticationService authenticationService,
            ILogger<NotificatieController> logger)
        {
            _notificatieRepository = notificatieRepository;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notificatie>> GetNotificatieById(int id)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Fetching notificatie {NotificatieId} for user ID: {UserId}", id, userId);
            var notificatie = await _notificatieRepository.GetNotificatieByIdAsync(id);
            if (notificatie == null || notificatie.UserId != userId)
            {
                return NotFound();
            }
            return Ok(notificatie);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notificatie>>> GetAllNotificaties()
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Fetching all notificaties for user ID: {UserId}", userId);

            // Get notificaties for the current user
            var notificaties = await _notificatieRepository.GetNotificatiesByUserIdAsync(userId);

            return Ok(notificaties);
        }

        [HttpPost]
        public async Task<ActionResult> AddNotificatie(Notificatie notificatie)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Adding new notificatie for user ID: {UserId}", userId);

            // Assign the current user ID to the notificatie
            notificatie.UserId = userId;

            await _notificatieRepository.AddNotificatieAsync(notificatie);
            return CreatedAtAction(nameof(GetNotificatieById), new { id = notificatie.ID }, notificatie);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateNotificatie(int id, Notificatie notificatie)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            if (id != notificatie.ID)
            {
                return BadRequest();
            }

            _logger.LogInformation("Updating notificatie {NotificatieId} for user ID: {UserId}", id, userId);

            // Verify the user has permission to update this notificatie
            var existingNotificatie = await _notificatieRepository.GetNotificatieByIdAsync(id);
            if (existingNotificatie == null) return NotFound();
            if (existingNotificatie.UserId != userId) return Forbid();

            notificatie.UserId = userId;
            await _notificatieRepository.UpdateNotificatieAsync(notificatie);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotificatie(int id)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Deleting notificatie {NotificatieId} for user ID: {UserId}", id, userId);

            // Verify the user has permission to delete this notificatie
            var existingNotificatie = await _notificatieRepository.GetNotificatieByIdAsync(id);
            if (existingNotificatie == null) return NotFound();
            if (existingNotificatie.UserId != userId) return Forbid();

            await _notificatieRepository.DeleteNotificatieAsync(id);
            return NoContent();
        }
    }
}
