using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface IWorkoutSchemaReader
    {
        Task<List<WorkoutSchema>> GetWorkoutSchemasByAthleteIdAsync(int athleteId);
        Task<WorkoutSchema> GetWorkoutSchemaByIdAsync(int id);
    }
} 