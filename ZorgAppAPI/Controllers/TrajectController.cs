using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrajectController : ControllerBase
    {
        private readonly ITrajectRepository _trajectRepository;

        public TrajectController(ITrajectRepository trajectRepository)
        {
            _trajectRepository = trajectRepository;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTrajectById(int id)
        {
            var traject = await _trajectRepository.GetTrajectByIdAsync(id);
            if (traject == null)
            {
                return NotFound();
            }
            return Ok(traject);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTrajects()
        {
            var trajects = await _trajectRepository.GetAllTrajectsAsync();
            return Ok(trajects);
        }

        [HttpPost]
        public async Task<IActionResult> AddTraject([FromBody] Traject traject)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            await _trajectRepository.AddTrajectAsync(traject);
            return CreatedAtAction(nameof(GetTrajectById), new { id = traject.ID }, traject);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTraject(int id, [FromBody] Traject traject)
        {
            if (id != traject.ID || !ModelState.IsValid)
            {
                return BadRequest();
            }
            await _trajectRepository.UpdateTrajectAsync(traject);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTraject(int id)
        {
            await _trajectRepository.DeleteTrajectAsync(id);
            return NoContent();
        }
    }
}
