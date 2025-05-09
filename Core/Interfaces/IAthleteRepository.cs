using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    /// <summary>
    /// Interface for Athlete repository operations
    /// </summary>
    public interface IAthleteRepository
    {
        /// <summary>
        /// Gets all athletes
        /// </summary>
        Task<List<Athlete>> GetAllAthletesAsync();
        
        /// <summary>
        /// Gets an athlete by their ID
        /// </summary>
        Task<Athlete> GetAthleteByIdAsync(int id);
        
        /// <summary>
        /// Gets an athlete by the associated user ID
        /// </summary>
        Task<Athlete> GetAthleteByUserIdAsync(int userId);
        
        /// <summary>
        /// Creates a new athlete in the database
        /// </summary>
        Task<int> CreateAthleteAsync(Athlete athlete);
        
        /// <summary>
        /// Updates an existing athlete in the database
        /// </summary>
        Task<bool> UpdateAthleteAsync(Athlete athlete);
        
        /// <summary>
        /// Deletes an athlete from the database
        /// </summary>
        Task<bool> DeleteAthleteAsync(int id);
    }
} 