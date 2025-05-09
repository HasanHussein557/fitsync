using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace ApiWrapper.Services;

public class GenerateNutritionSchema : INutritionSchemaGenerator
{
    private readonly HttpClient _httpClient;

    public GenerateNutritionSchema(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> GenerateNutritionPlanAsync(int weight, int height, int age, string sex, 
        string goal, string dietaryPreferences, bool hasAllergies, string foodIntolerances, int durationWeeks = 4)
    {
        var requestUrl = "http://127.0.0.1:8000/nutrition-plans/generate";

        // Validate input parameters
        if (string.IsNullOrEmpty(sex)) sex = "male";
        if (string.IsNullOrEmpty(goal)) goal = "maintenance";
        if (string.IsNullOrEmpty(dietaryPreferences)) dietaryPreferences = "balanced";
        if (weight < 20) weight = 70;
        if (height < 100) height = 170;
        if (age < 16) age = 30;
        if (durationWeeks < 1) durationWeeks = 4;

        // Create the request payload
        var payload = new
        {
            weight = (double)weight,
            height = (double)height,
            age,
            sex,
            goal,
            dietary_preferences = new[] { dietaryPreferences },
            food_intolerance = hasAllergies ? foodIntolerances.Split(',').Select(a => a.Trim()).ToArray() : Array.Empty<string>(),
            duration_weeks = durationWeeks
        };

        // Serialize the payload to JSON
        var jsonPayload = JsonSerializer.Serialize(payload);
        Console.WriteLine($"Sending request with payload: {jsonPayload}");

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
        
        try
        {
            // Make the POST request
            Console.WriteLine($"Sending request to: {requestUrl}");
            
            var response = await _httpClient.PostAsync(requestUrl, content);

            Console.WriteLine($"Response Status: {response.StatusCode}");
            
            // Read the response content even if it's an error
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response Content: {responseContent}");

            // Now check if the response was successful
            if (!response.IsSuccessStatusCode)
            {
                throw new ApplicationException($"API Error: {response.StatusCode} - {responseContent}");
            }

            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP Request Error: {ex.Message}");
            throw new ApplicationException($"Failed to connect to the nutrition plan service at {requestUrl}. Please ensure the API is running.", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error occurred: {ex.Message}");
            throw new ApplicationException("Error generating nutrition plan", ex);
        }
    }
} 