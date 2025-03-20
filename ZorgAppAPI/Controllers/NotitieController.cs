using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotitieController : ControllerBase
    {
        private readonly INotitieRepository _notitieRepository;

        public NotitieController(INotitieRepository notitieRepository)
        {
            _notitieRepository = notitieRepository;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Notitie>> GetNotitieById(int id)
        {
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
            var notities = await _notitieRepository.GetAllNotitiesAsync();
            return Ok(notities);
        }

        [HttpPost]
        public async Task<ActionResult> AddNotitie(Notitie notitie)
        {
            await _notitieRepository.AddNotitieAsync(notitie);
            return CreatedAtAction(nameof(GetNotitieById), new { id = notitie.ID }, notitie);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateNotitie(int id, Notitie notitie)
        {
            if (id != notitie.ID)
            {
                return BadRequest();
            }

            await _notitieRepository.UpdateNotitieAsync(notitie);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteNotitie(int id)
        {
            await _notitieRepository.DeleteNotitieAsync(id);
            return NoContent();
        }
    }
}
