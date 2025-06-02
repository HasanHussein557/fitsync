using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Core.Services;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiWrapper.Services;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using Core.Interfaces;
using System.Text.Json.Serialization.Metadata;
using System;

namespace Presentation.Pages;

[Authorize]
public class WorkoutSchemaModel : PageModel
{
    private readonly ILogger<WorkoutSchemaModel> _logger;
    private readonly IWorkoutSchemaGenerator _workoutSchemaGenerator;
    private readonly WorkoutSchemaService _workoutSchemaService;
    private readonly IAthleteService _athleteService;

    public WorkoutSchemaModel(
        ILogger<WorkoutSchemaModel> logger, 
        IWorkoutSchemaGenerator workoutSchemaGenerator,
        WorkoutSchemaService workoutSchemaService,
        IAthleteService athleteService)
    {
        _logger = logger;
        _workoutSchemaGenerator = workoutSchemaGenerator;
        _workoutSchemaService = workoutSchemaService;
        _athleteService = athleteService;
        
        // Set default values for required fields
        Sex = "male";
        Goal = "general_fitness";
        WorkoutPlan = new WorkoutPlanModel();
        StatusMessage = string.Empty;
        IsLoading = false;
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
    public int FitnessLevel { get; set; } = 2; // Default to intermediate

    [BindProperty]
    public int WorkoutsPerWeek { get; set; } = 3; // Default to 3 workouts per week
    
    public WorkoutPlanModel WorkoutPlan { get; private set; }

    [TempData]
    public string StatusMessage { get; set; }

    [TempData]
    public bool IsLoading { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            // Get current user ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            // Get the athlete data using the AthleteService
            var athlete = await _athleteService.GetAthleteForUserAsync(userId);
            
            if (athlete != null)
            {
                // Pre-fill form with athlete data
                Weight = athlete.Weight;
                Height = athlete.Height;
                Age = athlete.Age;
                Sex = athlete.Sex.ToLower();
                
                // Map athlete goal to a standardized workout goal
                Goal = athlete.Goal.ToLower() switch
                {
                    "weight loss" => "fat_loss",
                    "muscle gain" => "muscle_gain",
                    "maintenance" => "general_fitness",
                    "performance" => "athletic_performance",
                    _ => "general_fitness"
                };
            }
            else
            {
                // Default values if no athlete data is available
                SetDefaultValues();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading athlete data");
            SetDefaultValues();
        }
    }

    private void SetDefaultValues()
    {
        Weight = 70;
        Height = 170;
        Age = 30;
        Sex = "male";
        Goal = "general_fitness";
        FitnessLevel = 2;
        WorkoutsPerWeek = 3;
    }

    // Generate the workout plan
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            IsLoading = true;
            
            // Log the request parameters
            _logger.LogInformation("Generating workout schema - Weight: {Weight}, Height: {Height}, Age: {Age}, Sex: {Sex}, Goal: {Goal}, FitnessLevel: {FitnessLevel}, WorkoutsPerWeek: {WorkoutsPerWeek}", 
                Weight, Height, Age, Sex, Goal, FitnessLevel, WorkoutsPerWeek);
            
            // Call the service to generate the workout plan
            var response = await _workoutSchemaGenerator.GenerateWorkoutSchemaAsync(
                Weight, Height, Age, Sex, Goal, WorkoutsPerWeek);

            // Deserialize to our model
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            WorkoutPlan = JsonSerializer.Deserialize<WorkoutPlanModel>(response, options);

            // Pass the raw response to the view as well
            ViewData["WorkoutPlanJson"] = response;
            
            IsLoading = false;
            return Page();
        }
        catch (Exception ex)
        {
            IsLoading = false;
            _logger.LogError(ex, "Error generating workout plan");
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }

    // Save the workout plan
    public async Task<IActionResult> OnPostSaveAsync(string rawWorkoutPlan, string schemaName)
    {
        _logger.LogInformation("OnPostSaveAsync called with rawWorkoutPlan length: {Length}, schemaName: {SchemaName}", 
            rawWorkoutPlan?.Length ?? 0, schemaName ?? "null");
        
        // Also log the ViewData content for debugging
        var viewDataJson = ViewData["WorkoutPlanJson"]?.ToString();
        _logger.LogInformation("ViewData WorkoutPlanJson length: {Length}", viewDataJson?.Length ?? 0);
        
        try
        {
            if (string.IsNullOrEmpty(rawWorkoutPlan))
            {
                _logger.LogWarning("No workout plan data provided to save");
                ModelState.AddModelError(string.Empty, "No workout plan data to save.");
                return Page();
            }

            // Log a sample of the rawWorkoutPlan for debugging
            var sampleData = rawWorkoutPlan.Length > 200 ? rawWorkoutPlan.Substring(0, 200) + "..." : rawWorkoutPlan;
            _logger.LogInformation("Raw workout plan sample: {Sample}", sampleData);

            // Get current user ID and athlete ID
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            _logger.LogInformation("Saving workout for user ID: {UserId}", userId);
            
            var athlete = await _athleteService.GetAthleteForUserAsync(userId);
            
            if (athlete == null)
            {
                _logger.LogWarning("No athlete profile found for user ID: {UserId}", userId);
                ModelState.AddModelError(string.Empty, "You need to create an athlete profile first.");
                return Page();
            }

            _logger.LogInformation("Found athlete ID: {AthleteId} for user ID: {UserId}", athlete.Id, userId);

            try
            {
                // Deserialize the workout plan to save
                WorkoutPlan = JsonSerializer.Deserialize<WorkoutPlanModel>(rawWorkoutPlan) ?? new WorkoutPlanModel();
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing workout plan: {raw}", rawWorkoutPlan);
                ModelState.AddModelError(string.Empty, $"Error parsing workout data: {ex.Message}");
                return Page();
            }

            // Ensure we have values for Sex and Goal
            Sex = string.IsNullOrEmpty(Sex) ? (athlete.Sex ?? "Male") : Sex;
            Goal = string.IsNullOrEmpty(Goal) ? (athlete.Goal ?? "Maintenance") : Goal;
            schemaName = string.IsNullOrEmpty(schemaName) ? $"{Goal} Workout - {DateTime.Now:yyyy-MM-dd}" : schemaName;

            _logger.LogInformation("Creating workout schema with name: {SchemaName}, Goal: {Goal}, Sex: {Sex}", 
                schemaName, Goal, Sex);

            // Convert from the presentation model to the domain model
            var workoutSchema = new Core.Domain.Entities.WorkoutSchema
            {
                AthleteId = athlete.Id,
                Name = schemaName,
                CreatedDate = DateTime.Now,
                WorkoutsPerWeek = WorkoutPlan.SessionsPerWeek,
                Goal = Goal,
                Workouts = new List<Core.Domain.Entities.Workout>()
            };

            _logger.LogInformation("Workout schema created with {WorkoutCount} workout sessions", 
                WorkoutPlan.WorkoutSessions.Count);

            // Convert each workout session to domain workout
            for (int i = 0; i < WorkoutPlan.WorkoutSessions.Count; i++)
            {
                var session = WorkoutPlan.WorkoutSessions[i];
                var workout = new Core.Domain.Entities.Workout
                {
                    Name = $"Workout {i + 1}",
                    DayOfWeek = i + 1,
                    Exercises = new List<Core.Domain.Entities.Exercise>()
                };

                _logger.LogInformation("Processing workout {WorkoutIndex} with {ExerciseCount} exercises", 
                    i + 1, session.Exercises.Count);

                // Convert each exercise
                foreach (var exercise in session.Exercises)
                {
                    // Ensure the Rest property is properly formatted
                    string restValue = exercise.Rest;
                    if (!restValue.EndsWith("s") && int.TryParse(restValue, out int restSeconds))
                    {
                        restValue = $"{restSeconds}s";
                    }
                    
                    workout.Exercises.Add(new Core.Domain.Entities.Exercise
                    {
                        Name = exercise.Name,
                        Sets = exercise.Sets,
                        Reps = exercise.Reps,
                        Rest = int.TryParse(restValue.Replace("s", ""), out var parsedRestValue) ? parsedRestValue.ToString() : "60"
                    });
                }

                workoutSchema.Workouts.Add(workout);
            }

            _logger.LogInformation("About to save workout schema to database...");

            // Save to the database
            var savedSchema = await _workoutSchemaService.SaveWorkoutSchemaAsync(workoutSchema);
            
            _logger.LogInformation("Workout schema saved successfully with ID: {SchemaId}", savedSchema.Id);
            
            StatusMessage = $"Workout schema '{schemaName}' successfully saved!";
            
            _logger.LogInformation("Redirecting to WorkoutSchemas page with status message: {StatusMessage}", StatusMessage);
            
            return RedirectToPage("/WorkoutSchemas");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving workout schema");
            string errorDetails = $"Error message: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorDetails += $" Inner error: {ex.InnerException.Message}";
            }
            Console.WriteLine("ERROR DETAILS: " + errorDetails);
            ModelState.AddModelError(string.Empty, $"An error occurred while saving: {errorDetails}");
            return Page();
        }
    }

    // Workout plan model
    public class WorkoutPlanModel
    {
        [JsonPropertyName("warmup")]
        public WorkoutComponent Warmup { get; set; } = new WorkoutComponent();
        
        [JsonPropertyName("cardio")]
        public WorkoutComponent Cardio { get; set; } = new WorkoutComponent();
        
        [JsonPropertyName("sessions_per_week")]
        public int SessionsPerWeek { get; set; } = 0;
        
        [JsonPropertyName("workout_sessions")]
        public List<WorkoutSession> WorkoutSessions { get; set; } = new List<WorkoutSession>();
        
        [JsonPropertyName("cooldown")]
        public WorkoutComponent Cooldown { get; set; } = new WorkoutComponent();
    }
    
    // Workout component
    public class WorkoutComponent
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("exercises")]
        public List<string> Exercises { get; set; } = new List<string>();
        
        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; } = 0;
    }
    
    // Workout session
    public class WorkoutSession
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("target_muscles")]
        public List<string> TargetMuscles { get; set; } = new List<string>();
        
        [JsonPropertyName("exercises")]
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
        
        [JsonPropertyName("duration_minutes")]
        public int DurationMinutes { get; set; } = 0;
    }
    
    // Exercise
    public class Exercise
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("sets")]
        public int Sets { get; set; } = 0;
        
        [JsonPropertyName("reps")]
        public string Reps { get; set; } = string.Empty;
        
        [JsonPropertyName("rest")]
        [JsonConverter(typeof(RestConverter))]
        public string Rest { get; set; } = string.Empty;
    }
    
    // Custom converter for the Rest property to handle both string and integer values
    public class RestConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                int value = reader.GetInt32();
                return $"{value}s";
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                return reader.GetString() ?? string.Empty;
            }
            
            throw new JsonException($"Unexpected token type {reader.TokenType} for 'rest' property");
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }
} 