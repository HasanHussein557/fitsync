using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AthleteController : ControllerBase
    {
        private readonly IAthleteRepository _athleteRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AthleteController> _logger;

        public AthleteController(
            IAthleteRepository athleteRepository, 
            IUserRepository userRepository,
            ILogger<AthleteController> logger)
        {
            _athleteRepository = athleteRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAthleteById(int id)
        {
            try
            {
                // Verify that the current user has permission to access this athlete
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _userRepository.GetUserByIdAsync(userId);

                // Only admin or the owner can view an athlete profile
                if (!User.IsInRole("Admin") && user?.AthleteId != id)
                {
                    return Forbid();
                }

                var athlete = await _athleteRepository.GetAthleteByIdAsync(id);
                
                if (athlete == null)
                {
                    return NotFound();
                }
                
                return Ok(athlete);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving athlete with ID {AthleteId}", id);
                return StatusCode(500, "An error occurred while retrieving the athlete details.");
            }
        }
    }
} 