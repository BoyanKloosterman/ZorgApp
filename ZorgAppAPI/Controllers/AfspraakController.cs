using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Interfaces;
using ZorgAppAPI.Models;

namespace ZorgAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AfspraakController : ControllerBase
    {
        private readonly IAfspraakRepository _repository;

        public AfspraakController(IAfspraakRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Afspraak>>> GetAll()
        {
            var afspraken = await _repository.GetAllAsync();
            return Ok(afspraken);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Afspraak>> GetById(int id)
        {
            var afspraak = await _repository.GetByIdAsync(id);
            if (afspraak == null)
                return NotFound();
            return Ok(afspraak);
        }

        [HttpPost]
        public async Task<ActionResult> Create(Afspraak afspraak)
        {
            await _repository.AddAsync(afspraak);
            return CreatedAtAction(nameof(GetById), new { id = afspraak.ID }, afspraak);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Afspraak afspraak, [FromQuery] string userId, [FromQuery] bool isArts)
        {
            if (id != afspraak.ID)
                return BadRequest();

            try
            {
                await _repository.UpdateAsync(afspraak, userId, isArts);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id, [FromQuery] string userId, [FromQuery] bool isArts)
        {
            try
            {
                await _repository.DeleteAsync(id, userId, isArts);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
        }
    }
}
