using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using MySql.Data.MySqlClient;

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
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Users WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader((MySqlDataReader)reader);
                        }
                    }
                }
                return null;
            }
        }

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Users WHERE Username = @Username";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader((MySqlDataReader)reader);
                        }
                    }
                }
                return null;
            }
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM Users WHERE Email = @Email";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapUserFromReader((MySqlDataReader)reader);
                        }
                    }
                }
                return null;
            }
        }

        public async Task<User> CreateUserAsync(User user)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"INSERT INTO Users (Username, Email, PasswordHash, Salt, AthleteId, CreatedDate)
                            VALUES (@Username, @Email, @PasswordHash, @Salt, @AthleteId, @CreatedDate);
                            SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@Salt", user.Salt);
                    command.Parameters.AddWithValue("@AthleteId", user.AthleteId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@CreatedDate", DateTime.Now);

                    var id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    user.Id = id;

                    return user;
                }
            }
        }

        public async Task<bool> UpdateUserAsync(User user)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"UPDATE Users 
                            SET Username = @Username, 
                                Email = @Email, 
                                PasswordHash = @PasswordHash, 
                                Salt = @Salt, 
                                AthleteId = @AthleteId, 
                                LastLoginDate = @LastLoginDate
                            WHERE Id = @Id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", user.Id);
                    command.Parameters.AddWithValue("@Username", user.Username);
                    command.Parameters.AddWithValue("@Email", user.Email);
                    command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                    command.Parameters.AddWithValue("@Salt", user.Salt);
                    command.Parameters.AddWithValue("@AthleteId", user.AthleteId ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@LastLoginDate", user.LastLoginDate ?? (object)DBNull.Value);

                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "DELETE FROM Users WHERE Id = @Id";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }

        private User MapUserFromReader(MySqlDataReader reader)
        {
            return new User
            {
                Id = Convert.ToInt32(reader["Id"]),
                Username = reader["Username"].ToString(),
                Email = reader["Email"].ToString(),
                PasswordHash = reader["PasswordHash"].ToString(),
                Salt = reader["Salt"].ToString(),
                AthleteId = reader.IsDBNull(reader.GetOrdinal("AthleteId")) ? null : Convert.ToInt32(reader["AthleteId"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                LastLoginDate = reader.IsDBNull(reader.GetOrdinal("LastLoginDate")) ? null : Convert.ToDateTime(reader["LastLoginDate"])
            };
        }
    }
}