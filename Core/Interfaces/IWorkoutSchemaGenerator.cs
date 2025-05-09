using System.Threading.Tasks;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for generating workout schemas from external services
    /// </summary>
    public interface IWorkoutSchemaGenerator
    {
        /// <summary>
        /// Generates a workout plan based on user parameters
        /// </summary>
        /// <param name="weight">Weight in kg</param>
        /// <param name="height">Height in cm</param>
        /// <param name="age">Age in years</param>
        /// <param name="sex">Sex of the user</param>
        /// <param name="goal">Fitness goal</param>
        /// <param name="workoutsPerWeek">Number of workouts per week</param>
        /// <returns>JSON string containing the workout plan</returns>
        Task<string> GenerateWorkoutPlanAsync(int weight, int height, int age, string sex, string goal, int workoutsPerWeek);
    }
} 