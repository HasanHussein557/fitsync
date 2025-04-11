using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Core.Services
{
    public class WorkoutSchema
    {
        private readonly HttpClient _httpClient;

        public WorkoutSchema(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> GenerateWorkoutPlanAsync(int weight, int height, int age, string sex, string goal, int workoutsPerWeek)
        {
            var requestUrl = "http://127.0.0.1:8000/workout-plans/generate";

            // Create the request payload
            var payload = new
            {
                weight,
                height,
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
                var response = await _httpClient.PostAsync(requestUrl, content);

                Console.WriteLine($"Response: {response}");

                // Ensure the response is successful
                response.EnsureSuccessStatusCode();

                // Read and return the response content
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Received response: {responseContent}");
                return responseContent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                throw new ApplicationException("Error generating workout plan", ex);
            }
        }
    }
}