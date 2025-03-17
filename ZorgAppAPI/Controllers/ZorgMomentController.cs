using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ZorgMomentController : ControllerBase
    {
        private readonly IZorgmomentRepository _zorgmomentRepository;

        public ZorgMomentController(IZorgmomentRepository zorgmomentRepository)
        {
            _zorgmomentRepository = zorgmomentRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetZorgmomentById(int id)
        {
            var zorgmoment = await _zorgmomentRepository.GetZorgmomentByIdAsync(id);
            if (zorgmoment == null)
            {
                return NotFound();
            }
            return Ok(zorgmoment);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllZorgmomenten()
        {
            var zorgmomenten = await _zorgmomentRepository.GetAllZorgmomentenAsync();
            return Ok(zorgmomenten);
        }

        [HttpPost]
        public async Task<IActionResult> AddZorgmoment([FromBody] ZorgMoment zorgmoment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _zorgmomentRepository.AddZorgmomentAsync(zorgmoment);
            return CreatedAtAction(nameof(GetZorgmomentById), new { id = zorgmoment.ID }, zorgmoment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateZorgmoment(int id, [FromBody] ZorgMoment zorgmoment)
        {
            if (id != zorgmoment.ID || !ModelState.IsValid)
            {
                return BadRequest();
            }
            await _zorgmomentRepository.UpdateZorgmomentAsync(zorgmoment);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteZorgmoment(int id)
        {
            await _zorgmomentRepository.DeleteZorgmomentAsync(id);
            return NoContent();
        }
    }
}
