using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Presentation.Pages;

public class GenerateNutritionModel : PageModel
{
    private readonly ILogger<GenerateNutritionModel> _logger;

    public GenerateNutritionModel(ILogger<GenerateNutritionModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
    }
}