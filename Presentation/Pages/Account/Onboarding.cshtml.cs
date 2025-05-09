using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Presentation.Pages.Account
{
    [Authorize]
    public class OnboardingModel : PageModel
    {
        private readonly IAthleteService _athleteService;
        private readonly ILogger<OnboardingModel> _logger;

        public OnboardingModel(
            IAthleteService athleteService,
            ILogger<OnboardingModel> logger)
        {
            _athleteService = athleteService;
            _logger = logger;
        }

        [BindProperty]
        public Athlete Athlete { get; set; } = new Athlete();

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                // Check if user already has an athlete profile
                var existingAthlete = await _athleteService.GetAthleteForUserAsync(userId);
                
                if (existingAthlete != null)
                {
                    // User already has an athlete profile
                    TempData["InfoMessage"] = "You already have a fitness profile set up.";
                    return RedirectToPage("/Account/Profile");
                }

                // Pre-fill name with username if available
                Athlete.Name = User.Identity.Name ?? "";
                
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading onboarding page");
                TempData["ErrorMessage"] = "An error occurred while preparing your profile setup.";
                return RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Get current user ID
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                // Create athlete profile using the service
                _logger.LogInformation("Creating athlete profile for user {UserId}", userId);
                await _athleteService.CreateAthleteProfileAsync(Athlete, userId);
                
                TempData["SuccessMessage"] = "Your fitness profile has been successfully created!";
                return RedirectToPage("/Account/Profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during onboarding for user {UserId}", User.FindFirstValue(ClaimTypes.NameIdentifier));
                ModelState.AddModelError(string.Empty, "An error occurred while setting up your profile. Please try again.");
                return Page();
            }
        }
    }
} 