using System;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;

namespace Core.Services
{
    public class AthleteService : IAthleteService
    {
        private readonly IAthleteRepository _athleteRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;

        public AthleteService(
            IAthleteRepository athleteRepository,
            IUserRepository userRepository,
            IAuthService authService)
        {
            _athleteRepository = athleteRepository;
            _userRepository = userRepository;
            _authService = authService;
        }

        public async Task<Athlete> GetAthleteForUserAsync(int userId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || !user.AthleteId.HasValue)
                return null;

            return await _athleteRepository.GetAthleteByIdAsync(user.AthleteId.Value);
        }

        public async Task<int> CreateAthleteProfileAsync(Athlete athlete, int userId)
        {
            // Create the athlete
            var athleteId = await _athleteRepository.CreateAthleteAsync(athlete);
            
            // Link to user
            await _authService.LinkUserToAthleteAsync(userId, athleteId);
            
            return athleteId;
        }

        public async Task<bool> UpdateAthleteProfileAsync(Athlete athlete, int userId)
        {
            // Verify ownership
            if (!await UserOwnsAthleteAsync(userId, athlete.Id))
                throw new UnauthorizedAccessException("User does not own this athlete profile");
                
            return await _athleteRepository.UpdateAthleteAsync(athlete);
        }

        public async Task<bool> DeleteAthleteProfileAsync(int athleteId, int userId)
        {
            // Verify ownership
            if (!await UserOwnsAthleteAsync(userId, athleteId))
                throw new UnauthorizedAccessException("User does not own this athlete profile");
                
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user != null)
            {
                // Remove link from user to athlete
                user.AthleteId = null;
                await _userRepository.UpdateUserAsync(user);
            }
            
            return await _athleteRepository.DeleteAthleteAsync(athleteId);
        }

        public async Task<bool> UserOwnsAthleteAsync(int userId, int athleteId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            return user != null && user.AthleteId.HasValue && user.AthleteId.Value == athleteId;
        }
    }
} 