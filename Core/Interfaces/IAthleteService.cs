using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface IAthleteService
    {
        Task<Athlete> GetAthleteForUserAsync(int userId);
        Task<int> CreateAthleteProfileAsync(Athlete athlete, int userId);
        Task<bool> UpdateAthleteProfileAsync(Athlete athlete, int userId);
        Task<bool> DeleteAthleteProfileAsync(int athleteId, int userId);
        Task<bool> UserOwnsAthleteAsync(int userId, int athleteId);
    }
} 