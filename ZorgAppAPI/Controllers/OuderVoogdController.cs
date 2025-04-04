using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OuderVoogdController : ControllerBase
    {
        private readonly IOuderVoogdRepository _ouderVoogdRepository;
        private readonly IAuthenticationService _authenticationService;
        public OuderVoogdController(IOuderVoogdRepository ouderVoogdRepository, IAuthenticationService authenticationService)
        {
            _ouderVoogdRepository = ouderVoogdRepository;
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetOuderVoogden()
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }
            var ouderVoogden = await _ouderVoogdRepository.GetOuderVoogden();
            return Ok(ouderVoogden);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOuderVoogd(int id)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User not authenticated.");
            }
            var ouderVoogd = await _ouderVoogdRepository.GetOuderVoogd(id);
            if (ouderVoogd == null)
            {
                return NotFound();
            }
            return Ok(ouderVoogd);
        }

        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentOuderVoogd()
        {
            // Get the current user ID from the HttpContext
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User is not authenticated");
            }

            var ouderVoogd = await _ouderVoogdRepository.GetCurrentOuderVoogd(userId);
            if (ouderVoogd == null)
            {
                return NotFound();
            }
            return Ok(ouderVoogd);
        }
    }
}
