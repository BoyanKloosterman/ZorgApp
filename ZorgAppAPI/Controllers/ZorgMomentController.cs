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

        public ZorgMomentController(
            IZorgmomentRepository zorgmomentRepository,
            IAuthenticationService authenticationService)
        {
            _zorgmomentRepository = zorgmomentRepository;
            _authenticationService = authenticationService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetZorgmomentById(int id)
        {
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
            return Ok(zorgMomentIds); // Stuur de lijst met IDs terug
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

