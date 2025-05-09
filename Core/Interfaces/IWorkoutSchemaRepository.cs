using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for WorkoutSchema repository operations
    /// </summary>
    public interface IWorkoutSchemaRepository : IWorkoutSchemaReader, IWorkoutSchemaWriter
    {
        // This interface combines IWorkoutSchemaReader and IWorkoutSchemaWriter
        // No additional methods needed
    }
} 