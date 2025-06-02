using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface IAthleteRepository
    {
        Task<List<Athlete>> GetAllAthletesAsync();
        
        Task<Athlete> GetAthleteByIdAsync(int id);
        
        Task<Athlete> GetAthleteByUserIdAsync(int userId);
        
        Task<int> CreateAthleteAsync(Athlete athlete);
        
        Task<bool> UpdateAthleteAsync(Athlete athlete);
        
        Task<bool> DeleteAthleteAsync(int id);
    }
} 