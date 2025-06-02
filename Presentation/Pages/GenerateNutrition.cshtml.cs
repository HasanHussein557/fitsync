using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using System.Text.Json.Serialization;
using ApiWrapper.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Core.Interfaces;

namespace Presentation.Pages;

[Authorize]
public class GenerateNutritionModel : PageModel
{
    private readonly ILogger<GenerateNutritionModel> _logger;
    private readonly INutritionSchemaGenerator _nutritionSchemaGenerator;
    private readonly IAthleteService _athleteService;

    public GenerateNutritionModel(
        ILogger<GenerateNutritionModel> logger, 
        INutritionSchemaGenerator nutritionSchemaGenerator,
        IAthleteService athleteService)
    {
        _logger = logger;
        _nutritionSchemaGenerator = nutritionSchemaGenerator;
        _athleteService = athleteService;
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
    public string DietaryPreferences { get; set; }

    [BindProperty]
    public bool HasAllergies { get; set; }

    [BindProperty]
    public string FoodIntolerances { get; set; }
    
    [BindProperty]
    public int DurationWeeks { get; set; } = 4; // Default to 4 weeks
    
    public NutritionPlanModel NutritionPlan { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            // Get current user's athlete data if available
            if (User.Identity.IsAuthenticated)
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                var athlete = await _athleteService.GetAthleteForUserAsync(userId);
                
                if (athlete != null)
                {
                    // Pre-fill form with athlete data
                    Weight = athlete.Weight;
                    Height = athlete.Height;
                    Age = athlete.Age;
                    Sex = athlete.Sex.ToLower() == "male" ? "male" : 
                         athlete.Sex.ToLower() == "female" ? "female" : "other";
                    
                    // Map athlete goal to nutrition goal
                    Goal = athlete.Goal.ToLower() switch
                    {
                        "weight loss" => "weight_loss",
                        "muscle gain" => "muscle_gain",
                        "maintenance" => "maintenance",
                        "performance" => "performance",
                        _ => "maintenance"
                    };
                    
                    // Default diet type
                    DietaryPreferences = "balanced";
                    HasAllergies = false;
                    FoodIntolerances = "";
                    
                    return;
                }
            }
            
            // Default values if no athlete data is available
            SetDefaultValues();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading athlete data");
            // Use defaults
            SetDefaultValues();
        }
    }

    private void SetDefaultValues()
    {
        Weight = 70;
        Height = 170;
        Age = 30;
        Sex = "male";
        Goal = "maintenance";
        DietaryPreferences = "balanced";
        HasAllergies = false;
        FoodIntolerances = "";
        DurationWeeks = 4;
    }

    public async Task<IActionResult> OnPostGenerateNutrition()
    {
        try
        {
            // Log the request parameters
            _logger.LogInformation("Generating nutrition plan - Weight: {Weight}, Height: {Height}, Age: {Age}, Sex: {Sex}, Goal: {Goal}, Dietary Preferences: {DietaryPreferences}, Has Allergies: {HasAllergies}, Food Intolerances: {FoodIntolerances}, Duration Weeks: {DurationWeeks}", 
                Weight, Height, Age, Sex, Goal, DietaryPreferences, HasAllergies, FoodIntolerances, DurationWeeks);
            
            // Call the service to generate the nutrition plan
            var response = await _nutritionSchemaGenerator.GenerateNutritionSchemaAsync(
                Weight, Height, Age, Sex, Goal, DietaryPreferences, HasAllergies, FoodIntolerances, DurationWeeks);

            // Log the full response to diagnose JSON structure
            _logger.LogInformation("Nutrition Plan Response received: {Response}", response);

            try {
                // Configure JSON serializer options to be more tolerant
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    ReadCommentHandling = JsonCommentHandling.Skip
                };

                // Deserialize to our model with options
                NutritionPlan = JsonSerializer.Deserialize<NutritionPlanModel>(response, options);

                // Pass the raw response to the view as well
                ViewData["NutritionPlanJson"] = response;
                
                return Page();
            }
            catch (JsonException ex)
            {
                // Log the specific JSON parsing error
                _logger.LogError(ex, "JSON Deserialization Error: {ErrorMessage}. JSON: {JSON}", ex.Message, response);
                ModelState.AddModelError(string.Empty, $"Error parsing nutrition plan: {ex.Message}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating nutrition plan");
            ModelState.AddModelError(string.Empty, $"An error occurred: {ex.Message}");
            return Page();
        }
    }
    
    // New model classes that match the API structure
    public class NutritionPlanModel
    {
        [JsonPropertyName("daily_calories_range")]
        public CaloriesRange DailyCaloriesRange { get; set; } = new CaloriesRange();
        
        [JsonPropertyName("macronutrients_range")]
        public Dictionary<string, NutrientRange> MacronutrientsRange { get; set; } = new Dictionary<string, NutrientRange>();
        
        [JsonPropertyName("meal_plan")]
        public MealPlanStructure MealPlan { get; set; } = new MealPlanStructure();
        
        // Helper methods for the view
        public int GetAverageDailyCalories()
        {
            return (DailyCaloriesRange.Min + DailyCaloriesRange.Max) / 2;
        }
        
        public int GetAverageMacroValue(string macro)
        {
            if (MacronutrientsRange.TryGetValue(macro, out var range))
            {
                return (range.Min + range.Max) / 2;
            }
            return 0;
        }
        
        public int GetTotalMealCalories()
        {
            int total = 0;
            
            if (MealPlan.Breakfast != null)
                total += MealPlan.Breakfast.Sum(m => m.TotalCalories);
                
            if (MealPlan.Lunch != null)
                total += MealPlan.Lunch.Sum(m => m.TotalCalories);
                
            if (MealPlan.Dinner != null)
                total += MealPlan.Dinner.Sum(m => m.TotalCalories);
                
            if (MealPlan.Snacks != null)
                total += MealPlan.Snacks.Sum(m => m.TotalCalories);
                
            return total;
        }
    }
    
    public class CaloriesRange
    {
        [JsonPropertyName("min")]
        public int Min { get; set; }
        
        [JsonPropertyName("max")]
        public int Max { get; set; }
    }
    
    public class NutrientRange
    {
        [JsonPropertyName("min")]
        public int Min { get; set; }
        
        [JsonPropertyName("max")]
        public int Max { get; set; }
    }
    
    public class MealPlanStructure
    {
        [JsonPropertyName("breakfast")]
        public List<MealItem> Breakfast { get; set; } = new List<MealItem>();
        
        [JsonPropertyName("lunch")]
        public List<MealItem> Lunch { get; set; } = new List<MealItem>();
        
        [JsonPropertyName("dinner")]
        public List<MealItem> Dinner { get; set; } = new List<MealItem>();
        
        [JsonPropertyName("snacks")]
        public List<MealItem> Snacks { get; set; } = new List<MealItem>();
    }
    
    public class MealItem
    {
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
        
        [JsonPropertyName("ingredients")]
        public List<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
        
        [JsonPropertyName("total_calories")]
        public int TotalCalories { get; set; }
        
        [JsonPropertyName("recipe")]
        public string Recipe { get; set; } = string.Empty;
    }
    
    public class Ingredient
    {
        [JsonPropertyName("ingredient")]
        public string Name { get; set; } = string.Empty;
        
        [JsonPropertyName("quantity")]
        public string Quantity { get; set; } = string.Empty;
        
        [JsonPropertyName("calories")]
        public int Calories { get; set; }
    }
}