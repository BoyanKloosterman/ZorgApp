using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;
using ZorgAppAPI.Services;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZorgMomentController : ControllerBase
    {
        private readonly IZorgmomentRepository _zorgmomentRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<UserZorgMomentController> _logger;

        public ZorgMomentController(
            IZorgmomentRepository zorgmomentRepository,
            IAuthenticationService authenticationService,
            ILogger<UserZorgMomentController> logger)
        {
            _zorgmomentRepository = zorgmomentRepository;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetZorgmomentById(int id)
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Niet ingelogd");
            }
            var zorgmoment = await _zorgmomentRepository.GetZorgmomentByIdAsync(id);
            return zorgmoment != null ? Ok(zorgmoment) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetAllZorgmomenten()
        {
            var userId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Niet ingelogd");
            }

            var zorgMomentIds = await _zorgmomentRepository.GetAllZorgmomenten(userId);
            _logger.LogInformation($"ZorgMomentIds: {zorgMomentIds}");
            return Ok(zorgMomentIds); 
        }

        //[HttpPost]
        //public async Task<IActionResult> AddZorgmoment([FromBody] ZorgMoment zorgmoment)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    await _zorgmomentRepository.AddZorgmomentAsync(zorgmoment);
        //    return CreatedAtAction(nameof(GetZorgmomentById), new { id = zorgmoment.ID }, zorgmoment);
        //}

        //[HttpPut("{id}")]
        //public async Task<IActionResult> UpdateZorgmoment(int id, [FromBody] ZorgMoment zorgmoment)
        //{
        //    if (id != zorgmoment.ID || !ModelState.IsValid)
        //    {
        //        return BadRequest();
        //    }
        //    await _zorgmomentRepository.UpdateZorgmomentAsync(zorgmoment);
        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //public async Task<IActionResult> DeleteZorgmoment(int id)
        //{
        //    await _zorgmomentRepository.DeleteZorgmomentAsync(id);
        //    return NoContent();
        //}
    }
}

