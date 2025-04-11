using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Services;

namespace Presentation.Pages;

public class GenerateWorkoutModel : PageModel
{
    private readonly ILogger<GenerateWorkoutModel> _logger;
    private readonly WorkoutSchema _workoutSchema;

    public GenerateWorkoutModel(ILogger<GenerateWorkoutModel> logger, WorkoutSchema workoutSchema)
    {
        _logger = logger;
        _workoutSchema = workoutSchema;
    }

    [BindProperty]
    public int Weight { get; set; }

    [BindProperty]
    public int Height { get; set; }

    [BindProperty]
    public int Age { get; set; }

    [BindProperty]
    public string Sex { get; set; }

    [BindProperty]
    public string Goal { get; set; }

    [BindProperty]
    public int WorkoutsPerWeek { get; set; }

    public async Task<IActionResult> OnPostGenerateWorkout()
    {
        try
        {
            Console.WriteLine($"Generating workout - Weight: {Weight}, Height: {Height}, Age: {Age}, Sex: {Sex}, Goal: {Goal}, Workouts/week: {WorkoutsPerWeek}");
            
            // Call the service to generate the workout plan
            var response = await _workoutSchema.GenerateWorkoutPlanAsync(Weight, Height, Age, Sex, Goal, WorkoutsPerWeek);

            // Log the response
            Console.WriteLine($"Workout Plan Response received in controller: {response}");
            _logger.LogInformation($"Workout Plan Response: {response}");

            // Pass the response to the view
            ViewData["WorkoutPlan"] = response;
            return Page();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in controller: {ex.Message}");
            _logger.LogError(ex, "Error generating workout plan");
            ModelState.AddModelError(string.Empty, "An error occurred while generating the workout plan.");
            return Page();
        }
    }

    public void OnGet()
    {
    }
}