using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ZorgAppAPI.Interfaces;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OuderVoogdController : ControllerBase
    {
        private readonly IOuderVoogdRepository _ouderVoogdRepository;
        public OuderVoogdController(IOuderVoogdRepository ouderVoogdRepository)
        {
            _ouderVoogdRepository = ouderVoogdRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetOuderVoogden()
        {
            var ouderVoogden = await _ouderVoogdRepository.GetOuderVoogden();
            return Ok(ouderVoogden);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOuderVoogd(int id)
        {
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
