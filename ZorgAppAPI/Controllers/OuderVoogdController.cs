using Microsoft.AspNetCore.Mvc;
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
            var ouderVoogd = await _ouderVoogdRepository.GetCurrentOuderVoogd();
            if (ouderVoogd == null)
            {
                return NotFound();
            }
            return Ok(ouderVoogd);
        }
    }
}
