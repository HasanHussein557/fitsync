using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Core.Interfaces;
using System;
using System.Threading.Tasks;

namespace Presentation.Pages;

[Authorize]
public class WorkoutSchemaDetailsModel : PageModel
{
    private readonly ILogger<WorkoutSchemaDetailsModel> _logger;
    private readonly WorkoutSchemaService _workoutSchemaService;
    private readonly IAthleteService _athleteService;

    public WorkoutSchemaDetailsModel(
        ILogger<WorkoutSchemaDetailsModel> logger,
        WorkoutSchemaService workoutSchemaService,
        IAthleteService athleteService)
    {
        _logger = logger;
        _workoutSchemaService = workoutSchemaService;
        _athleteService = athleteService;
    }

    public WorkoutSchema WorkoutSchema { get; private set; }
    
    [TempData]
    public string StatusMessage { get; set; }
    
    public bool IsLoading { get; set; }
    
    public async Task<IActionResult> OnGetAsync(int id)
    {
        try
        {
            IsLoading = true;
            
            // Get current user ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get the athlete data
            var athlete = await _athleteService.GetAthleteForUserAsync(userId);
            
            if (athlete == null)
            {
                StatusMessage = "You need to create an athlete profile first.";
                return RedirectToPage("/Athletes/Create");
            }
            
            // Get the workout schema and verify it belongs to the athlete
            WorkoutSchema = await _workoutSchemaService.GetWorkoutSchemaByIdAsync(id);
            
            if (WorkoutSchema == null)
            {
                StatusMessage = "Workout schema not found.";
                return RedirectToPage("/WorkoutSchemas");
            }
            
            if (WorkoutSchema.AthleteId != athlete.Id)
            {
                StatusMessage = "You don't have permission to view this workout schema.";
                return RedirectToPage("/WorkoutSchemas");
            }
            
            IsLoading = false;
            return Page();
        }
        catch (Exception ex)
        {
            IsLoading = false;
            _logger.LogError(ex, "Error loading workout schema details");
            StatusMessage = $"An error occurred: {ex.Message}";
            return RedirectToPage("/WorkoutSchemas");
        }
    }
} 