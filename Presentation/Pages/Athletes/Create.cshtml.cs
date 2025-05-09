using System;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Presentation.Pages.Athletes
{
    public class CreateModel : PageModel
    {
        private readonly IAthleteRepository _athleteRepository;
        private readonly ILogger<CreateModel> _logger;

        public CreateModel(IAthleteRepository athleteRepository, ILogger<CreateModel> logger)
        {
            _athleteRepository = athleteRepository;
            _logger = logger;
        }

        [BindProperty]
        public Athlete Athlete { get; set; }

        public void OnGet()
        {
            // Initialize a new athlete
            Athlete = new Athlete();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }
                
                // Create the athlete in the database
                var athleteId = await _athleteRepository.CreateAthleteAsync(Athlete);
                
                if (athleteId > 0)
                {
                    _logger.LogInformation("New athlete created with ID {AthleteId}", athleteId);
                    return RedirectToPage("./Details", new { id = athleteId });
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Failed to create athlete");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating athlete");
                ModelState.AddModelError(string.Empty, $"An error occurred while creating the athlete: {ex.Message}");
                return Page();
            }
        }
    }
} 