using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using ApiWrapper.Services;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Services
{
    public class WorkoutSchemaService
    {
        private readonly IWorkoutSchemaRepository _workoutSchemaRepository;
        private readonly IAthleteRepository _athleteRepository;
        private readonly IWorkoutSchemaGenerator _generateWorkoutSchema;
        private readonly ILogger<WorkoutSchemaService> _logger;

        public WorkoutSchemaService(
            IWorkoutSchemaRepository workoutSchemaRepository,
            IAthleteRepository athleteRepository,
            IWorkoutSchemaGenerator generateWorkoutSchema,
            ILogger<WorkoutSchemaService> logger = null)
        {
            _workoutSchemaRepository = workoutSchemaRepository;
            _athleteRepository = athleteRepository;
            _generateWorkoutSchema = generateWorkoutSchema;
            _logger = logger;
        }

        public async Task<WorkoutSchema> GenerateAndSaveWorkoutSchemaAsync(int athleteId, int workoutsPerWeek)
        {
            // Get the athlete from the repository
            var athlete = await _athleteRepository.GetAthleteByIdAsync(athleteId);
            if (athlete == null)
            {
                throw new ArgumentException($"Athlete with ID {athleteId} not found.");
            }

            // Determine the gender for API call
            string gender = athlete.Sex?.ToLower() ?? "male";
            
            // Determine goal for API call
            string goal = athlete.Goal?.ToLower() ?? "strength";

            // Call the external service to generate a workout plan
            string workoutPlanJson = await _generateWorkoutSchema.GenerateWorkoutPlanAsync(
                athlete.Weight, 
                athlete.Height, 
                athlete.Age, 
                gender, 
                goal, 
                workoutsPerWeek);

            // Parse the workout plan response
            var workoutPlan = ParseWorkoutPlanResponse(workoutPlanJson);
            
            // Create a new workout schema entity
            var workoutSchema = new WorkoutSchema
            {
                AthleteId = athleteId,
                Name = $"{goal.ToUpper()} - {DateTime.Now.ToString("yyyy-MM-dd")}",
                CreatedDate = DateTime.Now,
                WorkoutsPerWeek = workoutsPerWeek,
                Goal = goal,
                Workouts = workoutPlan
            };

            // Save the workout schema to the database
            return await _workoutSchemaRepository.CreateWorkoutSchemaAsync(workoutSchema);
        }

        public async Task<WorkoutSchema> SaveWorkoutSchemaAsync(WorkoutSchema workoutSchema)
        {
            try
            {
                // Verify the athlete exists
                var athlete = await _athleteRepository.GetAthleteByIdAsync(workoutSchema.AthleteId);
                if (athlete == null)
                {
                    throw new ArgumentException($"Athlete with ID {workoutSchema.AthleteId} not found.");
                }

                // Ensure the created date is set
                if (workoutSchema.CreatedDate == default)
                {
                    workoutSchema.CreatedDate = DateTime.Now;
                }

                // Save the workout schema to the database
                return await _workoutSchemaRepository.CreateWorkoutSchemaAsync(workoutSchema);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error saving workout schema");
                Console.WriteLine($"ERROR SAVING WORKOUT SCHEMA: {ex.Message}");
                Console.WriteLine($"EXCEPTION TYPE: {ex.GetType().Name}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                }
                throw; // Rethrow to let the caller handle it
            }
        }

        public async Task<List<WorkoutSchema>> GetWorkoutSchemasForAthleteAsync(int athleteId)
        {
            try
            {
                return await _workoutSchemaRepository.GetWorkoutSchemasByAthleteIdAsync(athleteId);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving workout schemas for athlete {AthleteId}", athleteId);
                throw;
            }
        }

        public async Task<WorkoutSchema> GetWorkoutSchemaByIdAsync(int id)
        {
            try
            {
                return await _workoutSchemaRepository.GetWorkoutSchemaByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error retrieving workout schema with ID {Id}", id);
                throw;
            }
        }
        
        public async Task<bool> DeleteWorkoutSchemaAsync(int id)
        {
            try
            {
                return await _workoutSchemaRepository.DeleteWorkoutSchemaAsync(id);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting workout schema with ID {Id}", id);
                throw;
            }
        }

        private List<Workout> ParseWorkoutPlanResponse(string workoutPlanJson)
        {
            try
            {
                // Deserialize the JSON response into a dynamic object
                var jsonDoc = JsonDocument.Parse(workoutPlanJson);
                var root = jsonDoc.RootElement;
                
                // Check if the response contains a workouts property
                if (!root.TryGetProperty("workout_sessions", out var workoutsElement))
                {
                    throw new FormatException("Workout plan response does not contain a 'workout_sessions' property");
                }

                var workouts = new List<Workout>();
                int dayIndex = 1;

                // Iterate through the workouts array
                foreach (var workoutElement in workoutsElement.EnumerateArray())
                {
                    var workout = new Workout
                    {
                        Name = $"Workout {dayIndex}",
                        DayOfWeek = dayIndex,
                        Exercises = new List<Exercise>()
                    };

                    // Parse exercises if available
                    if (workoutElement.TryGetProperty("exercises", out var exercisesElement))
                    {
                        foreach (var exerciseElement in exercisesElement.EnumerateArray())
                        {
                            var exercise = new Exercise
                            {
                                Name = exerciseElement.TryGetProperty("name", out var exNameElement) 
                                    ? exNameElement.GetString() 
                                    : "Unknown Exercise",
                                Sets = exerciseElement.TryGetProperty("sets", out var setsElement) 
                                    ? setsElement.GetInt32() 
                                    : 3,
                                Reps = exerciseElement.TryGetProperty("reps", out var repsElement) 
                                    ? repsElement.GetString() 
                                    : "10-12",
                                Rest = exerciseElement.TryGetProperty("rest", out var restElement) 
                                    ? restElement.GetString() 
                                    : "60s"
                            };
                            
                            workout.Exercises.Add(exercise);
                        }
                    }

                    workouts.Add(workout);
                    dayIndex++;
                }

                return workouts;
            }
            catch (JsonException ex)
            {
                throw new FormatException($"Failed to parse workout plan JSON: {ex.Message}", ex);
            }
        }
    }
} 