using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages.Athletes
{
    public class IndexModel : PageModel
    {
        private readonly IAthleteRepository _athleteRepository;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IAthleteRepository athleteRepository, ILogger<IndexModel> logger)
        {
            _athleteRepository = athleteRepository;
            _logger = logger;
        }

        public List<Athlete> Athletes { get; set; } = new List<Athlete>();

        public async Task OnGetAsync()
        {
            try
            {
                var athletes = await _athleteRepository.GetAllAthletesAsync();
                if (athletes != null)
                {
                    Athletes = athletes;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching athletes");
                ModelState.AddModelError(string.Empty, "An error occurred while retrieving the athletes.");
            }
        }
    }
} 