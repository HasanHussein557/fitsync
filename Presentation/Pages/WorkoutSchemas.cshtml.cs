using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Presentation.Pages;

[Authorize]
public class WorkoutSchemasModel : PageModel
{
    private readonly ILogger<WorkoutSchemasModel> _logger;
    private readonly WorkoutSchemaService _workoutSchemaService;
    private readonly IAthleteService _athleteService;

    public WorkoutSchemasModel(
        ILogger<WorkoutSchemasModel> logger,
        WorkoutSchemaService workoutSchemaService,
        IAthleteService athleteService)
    {
        _logger = logger;
        _workoutSchemaService = workoutSchemaService;
        _athleteService = athleteService;
    }

    public List<WorkoutSchema> SavedWorkouts { get; private set; } = new List<WorkoutSchema>();
    
    [TempData]
    public string StatusMessage { get; set; }
    
    public bool IsLoading { get; set; }
    
    public async Task<IActionResult> OnGetAsync()
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
                StatusMessage = "You need to create an athlete profile first to view workout schemas.";
                return RedirectToPage("/Athletes/Create");
            }
            
            // Get all workout schemas for the athlete
            SavedWorkouts = await _workoutSchemaService.GetWorkoutSchemasForAthleteAsync(athlete.Id);
            
            IsLoading = false;
            return Page();
        }
        catch (Exception ex)
        {
            IsLoading = false;
            _logger.LogError(ex, "Error loading workout schemas");
            StatusMessage = $"An error occurred: {ex.Message}";
            return Page();
        }
    }
    
    public async Task<IActionResult> OnGetDetailsAsync(int id)
    {
        try
        {
            // Get current user ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get the athlete data
            var athlete = await _athleteService.GetAthleteForUserAsync(userId);
            
            if (athlete == null)
            {
                StatusMessage = "You need to create an athlete profile first.";
                return RedirectToPage("/Athletes/Create");
            }
            
            // Get specific workout schema and verify it belongs to the athlete
            var schema = await _workoutSchemaService.GetWorkoutSchemaByIdAsync(id);
            
            if (schema == null || schema.AthleteId != athlete.Id)
            {
                StatusMessage = "Workout schema not found or you don't have permission to view it.";
                return RedirectToPage();
            }
            
            // Return to the details section with the ID as a fragment
            return RedirectToPage("/WorkoutSchemaDetails", new { id = schema.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading workout schema details");
            StatusMessage = $"An error occurred: {ex.Message}";
            return RedirectToPage();
        }
    }
    
    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        try
        {
            // Get current user ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get the athlete data
            var athlete = await _athleteService.GetAthleteForUserAsync(userId);
            
            if (athlete == null)
            {
                StatusMessage = "You need to create an athlete profile first.";
                return RedirectToPage("/Athletes/Create");
            }
            
            // Get specific workout schema and verify it belongs to the athlete
            var schema = await _workoutSchemaService.GetWorkoutSchemaByIdAsync(id);
            
            if (schema == null || schema.AthleteId != athlete.Id)
            {
                StatusMessage = "Workout schema not found or you don't have permission to delete it.";
                return RedirectToPage();
            }
            
            // Delete the workout schema
            await _workoutSchemaService.DeleteWorkoutSchemaAsync(id);
            
            StatusMessage = "Workout schema deleted successfully.";
            return RedirectToPage();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting workout schema");
            StatusMessage = $"An error occurred: {ex.Message}";
            return RedirectToPage();
        }
    }
} 