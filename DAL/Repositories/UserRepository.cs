using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Npgsql;

namespace DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM users WHERE id = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader(reader);
                        }
                    }
                }
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM users WHERE username = @username";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader(reader);
                        }
                    }
                }
                return null;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM users WHERE email = @email";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader(reader);
                        }
                    }
                }
                return null;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Start transaction
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        // Insert user
                        var query = @"
                            INSERT INTO users (username, email, password_hash, salt, athlete_id, created_date, last_login_date) 
                            VALUES (@username, @email, @passwordHash, @salt, @athleteId, @createdDate, @lastLoginDate)
                            RETURNING id";

                        using (var command = new NpgsqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@username", user.Username);
                            command.Parameters.AddWithValue("@email", user.Email);
                            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                            command.Parameters.AddWithValue("@salt", user.Salt);
                            command.Parameters.AddWithValue("@athleteId", user.AthleteId.HasValue ? (object)user.AthleteId.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@createdDate", user.CreatedDate);
                            command.Parameters.AddWithValue("@lastLoginDate", user.LastLoginDate.HasValue ? (object)user.LastLoginDate.Value : DBNull.Value);

                            user.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                        }

                        // Insert roles if any
                        if (user.Roles != null && user.Roles.Count > 0)
                        {
                            foreach (var role in user.Roles)
                            {
                                var roleQuery = @"
                                    INSERT INTO user_roles (user_id, role)
                                    VALUES (@userId, @role)";

                                using (var command = new NpgsqlCommand(roleQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@userId", user.Id);
                                    command.Parameters.AddWithValue("@role", role);
                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return user;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                // Start transaction
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var query = @"
                            UPDATE users
                            SET username = @username,
                                email = @email,
                                password_hash = @passwordHash,
                                salt = @salt,
                                athlete_id = @athleteId,
                                last_login_date = @lastLoginDate
                            WHERE id = @id";

                        using (var command = new NpgsqlCommand(query, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@id", user.Id);
                            command.Parameters.AddWithValue("@username", user.Username);
                            command.Parameters.AddWithValue("@email", user.Email);
                            command.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
                            command.Parameters.AddWithValue("@salt", user.Salt);
                            command.Parameters.AddWithValue("@athleteId", user.AthleteId.HasValue ? (object)user.AthleteId.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@lastLoginDate", user.LastLoginDate.HasValue ? (object)user.LastLoginDate.Value : DBNull.Value);

                            await command.ExecuteNonQueryAsync();
                        }

                        // Update roles if needed
                        if (user.Roles != null)
                        {
                            // Delete existing roles
                            var deleteRolesQuery = "DELETE FROM user_roles WHERE user_id = @userId";
                            using (var command = new NpgsqlCommand(deleteRolesQuery, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@userId", user.Id);
                                await command.ExecuteNonQueryAsync();
                            }

                            // Insert new roles
                            foreach (var role in user.Roles)
                            {
                                var insertRoleQuery = @"
                                    INSERT INTO user_roles (user_id, role)
                                    VALUES (@userId, @role)";

                                using (var command = new NpgsqlCommand(insertRoleQuery, connection, transaction))
                                {
                                    command.Parameters.AddWithValue("@userId", user.Id);
                                    command.Parameters.AddWithValue("@role", role);
                                    await command.ExecuteNonQueryAsync();
                                }
                            }
                        }

                        await transaction.CommitAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "DELETE FROM users WHERE id = @id";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        private User MapUserFromReader(NpgsqlDataReader reader)
        {
            var user = new User
            {
                Id = Convert.ToInt32(reader["id"]),
                Username = reader["username"]?.ToString() ?? string.Empty,
                Email = reader["email"]?.ToString() ?? string.Empty,
                PasswordHash = reader["password_hash"]?.ToString() ?? string.Empty,
                Salt = reader["salt"]?.ToString() ?? string.Empty,
                CreatedDate = Convert.ToDateTime(reader["created_date"])
            };

            if (reader["athlete_id"] != DBNull.Value)
                user.AthleteId = Convert.ToInt32(reader["athlete_id"]);

            if (reader["last_login_date"] != DBNull.Value)
                user.LastLoginDate = Convert.ToDateTime(reader["last_login_date"]);

            return user;
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var roles = new List<string>();
            
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT role FROM user_roles WHERE user_id = @userId";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string role = reader["role"]?.ToString();
                            if (!string.IsNullOrEmpty(role))
                            {
                                roles.Add(role);
                            }
                        }
                    }
                }
            }
            
            return roles;
        }
    }
}