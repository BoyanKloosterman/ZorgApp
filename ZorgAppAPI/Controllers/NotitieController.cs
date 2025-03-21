using Microsoft.AspNetCore.Mvc;
using System.Data;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotitieController : ControllerBase
    {
        private readonly INotitieRepository _notitieRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<NotitieController> _logger;

        public NotitieController(
            INotitieRepository notitieRepository,
            IAuthenticationService authenticationService,
            ILogger<NotitieController> logger)
        {
            _notitieRepository = notitieRepository;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notitie>> GetNotitieById(int id)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Fetching notitie {NotitieId} for user ID: {UserId}", id, userId);
            var notitie = await _notitieRepository.GetNotitieByIdAsync(id);
            if (notitie == null)
            {
                return NotFound();
            }
            return Ok(notitie);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notitie>>> GetAllNotities()
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Fetching all notities for user ID: {UserId}", userId);

            // Assuming you want to only get notities for the current user
            var notities = await _notitieRepository.GetAllNotitiesAsync();
            // Optionally filter by userId if needed
            //var notities = await _notitieRepository.GetNotitiesByUserIdAsync(userId);

            return Ok(notities);
        }

        [HttpPost]
        public async Task<ActionResult> AddNotitie(Notitie notitie)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Adding new notitie for user ID: {UserId}", userId);

            //Assign the current user ID to the notitie
            notitie.UserId = userId;

            await _notitieRepository.AddNotitieAsync(notitie);
            return CreatedAtAction(nameof(GetNotitieById), new { id = notitie.ID }, notitie);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateNotitie(int id, Notitie notitie)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            if (id != notitie.ID)
            {
                return BadRequest();
            }

            _logger.LogInformation("Updating notitie {NotitieId} for user ID: {UserId}", id, userId);

            // Optional: Verify the user has permission to update this notitie
            //var existingNotitie = await _notitieRepository.GetNotitieByIdAsync(id);
            //if (existingNotitie == null) return NotFound();
            //if (existingNotitie.UserId != userId) return Forbid();

            notitie.UserId = userId;
            await _notitieRepository.UpdateNotitieAsync(notitie);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotitie(int id)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }

            _logger.LogInformation("Deleting notitie {NotitieId} for user ID: {UserId}", id, userId);
            // Dont know for sure whether Forbid works or not, may be temporary
            // Optional: Verify the user has permission to delete this notitie
            var existingNotitie = await _notitieRepository.GetNotitieByIdAsync(id);
            if (existingNotitie == null) return NotFound();
            if (existingNotitie.UserId != userId) return Forbid();

            await _notitieRepository.DeleteNotitieAsync(id);
            return NoContent();
        }
    }
}