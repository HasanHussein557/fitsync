using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using MySql.Data.MySqlClient;

namespace DAL.Repositories
{
    public class AthleteRepository : IAthleteRepository
    {
        private readonly string _connectionString;

        public AthleteRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<List<Athlete>> GetAllAthletesAsync()
        {
            var athletes = new List<Athlete>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT * FROM Athletes", connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            athletes.Add(MapAthleteFromReader((MySqlDataReader)reader));
                        }
                    }
                }
            }
            
            return athletes;
        }

        public async Task<Athlete> GetAthleteByIdAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT * FROM Athletes WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapAthleteFromReader((MySqlDataReader)reader);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<Athlete> GetAthleteByUserIdAsync(int userId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // First, find the user's AthleteId from the Users table
                using (var command = new MySqlCommand("SELECT AthleteId FROM Users WHERE Id = @UserId", connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    var athleteId = await command.ExecuteScalarAsync();
                    
                    if (athleteId == null || athleteId == DBNull.Value)
                    {
                        return null;
                    }
                    
                    // Now get the athlete with that ID
                    return await GetAthleteByIdAsync(Convert.ToInt32(athleteId));
                }
            }
        }

        public async Task<int> CreateAthleteAsync(Athlete athlete)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"INSERT INTO Athletes (FirstName, LastName, Age, Height, Weight, Sex, Goal) 
                               VALUES (@FirstName, @LastName, @Age, @Height, @Weight, @Sex, @Goal);
                               SELECT LAST_INSERT_ID();";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    AddAthleteParameters(command, athlete);
                    athlete.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return athlete.Id;
                }
            }
        }

        public async Task<bool> UpdateAthleteAsync(Athlete athlete)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"UPDATE Athletes 
                               SET FirstName = @FirstName, 
                                   LastName = @LastName, 
                                   Age = @Age, 
                                   Height = @Height, 
                                   Weight = @Weight,
                                   Sex = @Sex,
                                   Goal = @Goal
                               WHERE Id = @Id";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    AddAthleteParameters(command, athlete);
                    command.Parameters.AddWithValue("@Id", athlete.Id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteAthleteAsync(int id)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("DELETE FROM Athletes WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        private Athlete MapAthleteFromReader(MySqlDataReader reader)
        {
            return new Athlete
            {
                Id = Convert.ToInt32(reader["Id"]),
                FirstName = reader["FirstName"].ToString(),
                LastName = reader["LastName"].ToString(),
                Age = reader.IsDBNull(reader.GetOrdinal("Age")) ? 0 : Convert.ToInt32(reader["Age"]),
                Height = reader.IsDBNull(reader.GetOrdinal("Height")) ? 0 : Convert.ToInt32(reader["Height"]),
                Weight = reader.IsDBNull(reader.GetOrdinal("Weight")) ? 0 : Convert.ToInt32(reader["Weight"]),
                Sex = reader.IsDBNull(reader.GetOrdinal("Sex")) ? "Male" : reader["Sex"].ToString(),
                Goal = reader.IsDBNull(reader.GetOrdinal("Goal")) ? "Maintenance" : reader["Goal"].ToString()
            };
        }

        private void AddAthleteParameters(MySqlCommand command, Athlete athlete)
        {
            command.Parameters.AddWithValue("@FirstName", athlete.FirstName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@LastName", athlete.LastName ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Age", athlete.Age == 0 ? 30 : athlete.Age);
            command.Parameters.AddWithValue("@Height", athlete.Height == 0 ? 170 : athlete.Height);
            command.Parameters.AddWithValue("@Weight", athlete.Weight == 0 ? 70 : athlete.Weight);
            command.Parameters.AddWithValue("@Sex", string.IsNullOrEmpty(athlete.Sex) ? "Male" : athlete.Sex);
            command.Parameters.AddWithValue("@Goal", string.IsNullOrEmpty(athlete.Goal) ? "Maintenance" : athlete.Goal);
        }
    }
}