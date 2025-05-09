using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface IAuthService
    {
        Task<User> RegisterUserAsync(string username, string email, string password);
        Task<User> AuthenticateAsync(string usernameOrEmail, string password);
        Task<bool> LinkUserToAthleteAsync(int userId, int athleteId);
        string GenerateJwtToken(User user);
        string HashPassword(string password, out string salt);
        bool VerifyPassword(string password, string hash, string salt);
    }
}