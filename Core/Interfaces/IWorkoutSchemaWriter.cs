using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for writing workout schemas to the data store
    /// </summary>
    public interface IWorkoutSchemaWriter
    {
        /// <summary>
        /// Creates a new workout schema in the database
        /// </summary>
        Task<WorkoutSchema> CreateWorkoutSchemaAsync(WorkoutSchema workoutSchema);
        
        /// <summary>
        /// Updates an existing workout schema in the database
        /// </summary>
        Task<WorkoutSchema> UpdateWorkoutSchemaAsync(WorkoutSchema workoutSchema);
        
        /// <summary>
        /// Deletes a workout schema from the database
        /// </summary>
        Task<bool> DeleteWorkoutSchemaAsync(int id);
    }
} 