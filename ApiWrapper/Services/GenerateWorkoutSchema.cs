using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Core.Interfaces;

namespace ApiWrapper.Services;

public class GenerateWorkoutSchema : IWorkoutSchemaGenerator
{
    private readonly HttpClient _httpClient;

    public GenerateWorkoutSchema(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
     public async Task<string> GenerateWorkoutPlanAsync(int weight, int height, int age, string sex, string goal, int workoutsPerWeek)
        {
            var requestUrl = "http://127.0.0.1:8000/workout-plans/generate";

            // Validate input parameters
            if (workoutsPerWeek < 1) workoutsPerWeek = 3;
            if (string.IsNullOrEmpty(sex)) sex = "male";
            if (string.IsNullOrEmpty(goal)) goal = "strength";
            if (weight < 20) weight = 70;
            if (height < 100) height = 170;
            if (age < 16) age = 30;

            // Create the request payload with proper types (double for weight and height)
            var payload = new
            {
                weight = (double)weight,
                height = (double)height,
                age,
                sex,
                goal,
                workouts_per_week = workoutsPerWeek
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
                throw new ApplicationException($"Failed to connect to the workout plan service at {requestUrl}. Please ensure the API is running.", ex);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                throw new ApplicationException("Error generating workout plan", ex);
            }
        }
}