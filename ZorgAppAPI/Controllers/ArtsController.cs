using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Interfaces;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArtsController : ControllerBase
    {
        private readonly IArtsRepository _artsRepository;
        public ArtsController(IArtsRepository artsRepository)
        {
            _artsRepository = artsRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetArts()
        {
            var arts = await _artsRepository.GetArts();
            return Ok(arts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetArts(int id)
        {
            var arts = await _artsRepository.GetArts(id);
            if (arts == null)
            {
                return NotFound();
            }
            return Ok(arts);
        }

    }
}
