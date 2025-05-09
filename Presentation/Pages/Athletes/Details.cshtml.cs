using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Presentation.Pages.Athletes
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IAthleteRepository _athleteRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(IAthleteRepository athleteRepository, IUserRepository userRepository, ILogger<DetailsModel> logger)
        {
            _athleteRepository = athleteRepository;
            _userRepository = userRepository;
            _logger = logger;
        }

        public Athlete Athlete { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                // If no ID is provided, try to get the athlete ID from the current user
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _userRepository.GetUserByIdAsync(userId);
                
                if (user?.AthleteId == null)
                {
                    TempData["ErrorMessage"] = "You don't have an athlete profile yet.";
                    return RedirectToPage("/Athletes/Create");
                }
                
                id = user.AthleteId.Value;
            }
            else
            {
                // If an ID is provided, make sure the user is authorized to view it
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var user = await _userRepository.GetUserByIdAsync(userId);
                
                // Only admin or the owner can view an athlete profile
                if (!User.IsInRole("Admin") && user?.AthleteId != id)
                {
                    TempData["ErrorMessage"] = "You are not authorized to view this athlete profile.";
                    return RedirectToPage("/Index");
                }
            }

            try
            {
                Athlete = await _athleteRepository.GetAthleteByIdAsync(id.Value);
                
                if (Athlete == null)
                {
                    _logger.LogWarning("Athlete with ID {AthleteId} not found", id);
                    return Page();
                }
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving athlete with ID {AthleteId}", id);
                ModelState.AddModelError(string.Empty, "An error occurred while retrieving the athlete details.");
                return Page();
            }
        }
    }
} 