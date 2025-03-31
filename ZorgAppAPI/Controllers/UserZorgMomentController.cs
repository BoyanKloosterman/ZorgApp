using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserZorgMomentController : ControllerBase
    {
        private readonly IUserZorgMomentRepository _userZorgMomentRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<UserZorgMomentController> _logger;

        public UserZorgMomentController(
            IUserZorgMomentRepository userZorgMomentRepository,
            IAuthenticationService authenticationService,
            ILogger<UserZorgMomentController> logger)
        {
            _userZorgMomentRepository = userZorgMomentRepository;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> AddUserZorgMoment([FromBody] ZorgmomentRequest request)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (userId == null)
            {
                return BadRequest("User ID is null");
            }
            else
            {
                _logger.LogInformation($"ZorgMomentId: {request.ZorgMomentId}");

                var createdZorgMoment = await _userZorgMomentRepository.AddUserZorgMomentAsync(userId, request.ZorgMomentId);

                return CreatedAtAction(nameof(GetUserZorgMomentsByUserId),
                    new { userId = createdZorgMoment.UserId },
                    createdZorgMoment);
            }
        }

        // tijdelijk ff slordig hier
        public class ZorgmomentRequest
        {
            public int ZorgMomentId { get; set; }
        }

        [HttpGet]
        public async Task<IActionResult> GetUserZorgMomentsByUserId()
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (userId == null)
            {
                return BadRequest("User ID is null");
            }

            var zorgMomentIds = await _userZorgMomentRepository.GetUserZorgMomentsByUserIdAsync(userId);
            _logger.LogInformation($"ZorgMoment IDs: {string.Join(", ", zorgMomentIds)}");

            return Ok(zorgMomentIds); // Geeft een array van ints terug bv. [2,3,4]
        }
    }
}
