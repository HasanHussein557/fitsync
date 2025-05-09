using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;

namespace Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly string _jwtSecretKey;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _jwtExpiryMinutes;

        public AuthService(IUserRepository userRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtSecretKey = configuration["Jwt:Key"] ?? "defaultSecureKeyWith32Characters!!!";
            _jwtIssuer = configuration["Jwt:Issuer"] ?? "fitSync";
            _jwtAudience = configuration["Jwt:Audience"] ?? "fitSync.Users";
            _jwtExpiryMinutes = configuration.GetValue<int>("Jwt:ExpiryMinutes", 60);
        }

        public async Task<User> RegisterUserAsync(string username, string email, string password)
        {
            // Check if user already exists
            var existingUser = await _userRepository.GetUserByUsernameAsync(username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username is already taken");
            }

            existingUser = await _userRepository.GetUserByEmailAsync(email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already registered");
            }

            // Create new user
            string salt;
            string hash = HashPassword(password, out salt);

            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = hash,
                Salt = salt,
                CreatedDate = DateTime.Now
            };

            // Save the user
            user = await _userRepository.CreateUserAsync(user);
            return user;
        }

        public async Task<User> AuthenticateAsync(string usernameOrEmail, string password)
        {
            // Try to find user by username or email
            User user = await _userRepository.GetUserByUsernameAsync(usernameOrEmail);
            if (user == null)
            {
                user = await _userRepository.GetUserByEmailAsync(usernameOrEmail);
            }

            if (user == null)
            {
                return null; // User not found
            }

            // Verify password
            if (!VerifyPassword(password, user.PasswordHash, user.Salt))
            {
                return null; // Invalid password
            }

            // Update last login date
            user.LastLoginDate = DateTime.Now;
            await _userRepository.UpdateUserAsync(user);

            return user;
        }

        public async Task<bool> LinkUserToAthleteAsync(int userId, int athleteId)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            user.AthleteId = athleteId;
            return await _userRepository.UpdateUserAsync(user);
        }

        public string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(_jwtExpiryMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            };

            // Add AthleteId claim if it exists
            if (user.AthleteId.HasValue)
            {
                claims.Add(new Claim("AthleteId", user.AthleteId.Value.ToString()));
            }

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string HashPassword(string password, out string salt)
        {
            // Generate a random salt
            byte[] saltBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }
            salt = Convert.ToBase64String(saltBytes);

            // Hash the password with the salt
            return ComputeHash(password, salt);
        }

        public bool VerifyPassword(string password, string hash, string salt)
        {
            string computedHash = ComputeHash(password, salt);
            return computedHash == hash;
        }

        private string ComputeHash(string password, string salt)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] saltBytes = Convert.FromBase64String(salt);
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

                // Combine password and salt
                byte[] combined = new byte[passwordBytes.Length + saltBytes.Length];
                Buffer.BlockCopy(passwordBytes, 0, combined, 0, passwordBytes.Length);
                Buffer.BlockCopy(saltBytes, 0, combined, passwordBytes.Length, saltBytes.Length);

                // Compute hash
                byte[] hashBytes = sha256.ComputeHash(combined);
                return Convert.ToBase64String(hashBytes);
            }
        }
    }
}