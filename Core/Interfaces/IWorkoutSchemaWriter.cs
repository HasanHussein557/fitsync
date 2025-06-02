using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface IWorkoutSchemaWriter
    {
 
        Task<WorkoutSchema> CreateWorkoutSchemaAsync(WorkoutSchema workoutSchema);
        
        Task<WorkoutSchema> UpdateWorkoutSchemaAsync(WorkoutSchema workoutSchema);
        
        Task<bool> DeleteWorkoutSchemaAsync(int id);
    }
} 