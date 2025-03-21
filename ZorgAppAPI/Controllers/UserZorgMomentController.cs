using Microsoft.AspNetCore.Mvc;
using ZorgAppAPI.Models;
using ZorgAppAPI.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZorgAppAPI.Interfaces;

namespace ZorgAppAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserZorgMomentController : ControllerBase
    {
        private readonly IUserZorgMomentRepository _userZorgMomentRepository;

        public UserZorgMomentController(IUserZorgMomentRepository userZorgMomentRepository)
        {
            _userZorgMomentRepository = userZorgMomentRepository;
        }

        [HttpPost]
        public async Task<IActionResult> AddUserZorgMoment([FromBody] UserZorgMoment userZorgMoment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var createdZorgMoment = await _userZorgMomentRepository.AddUserZorgMomentAsync(userZorgMoment);
            return CreatedAtAction(nameof(GetUserZorgMomentsByUserId), new { userId = createdZorgMoment.UserId }, createdZorgMoment);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserZorgMomentsByUserId(string userId)
        {
            var zorgMoments = await _userZorgMomentRepository.GetUserZorgMomentsByUserIdAsync(userId);
            return Ok(zorgMoments);
        }
    }
}