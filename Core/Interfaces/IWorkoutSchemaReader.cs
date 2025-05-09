using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for reading workout schemas from the data store
    /// </summary>
    public interface IWorkoutSchemaReader
    {
        /// <summary>
        /// Gets all workout schemas for an athlete
        /// </summary>
        Task<List<WorkoutSchema>> GetWorkoutSchemasByAthleteIdAsync(int athleteId);
        
        /// <summary>
        /// Gets a workout schema by its ID
        /// </summary>
        Task<WorkoutSchema> GetWorkoutSchemaByIdAsync(int id);
    }
} 