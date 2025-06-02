using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Npgsql;

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
            
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM athletes";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            athletes.Add(MapAthleteFromReader(reader));
                        }
                    }
                }
            }
            
            return athletes;
        }

        public async Task<Athlete> GetAthleteByIdAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = "SELECT * FROM athletes WHERE id = @id";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapAthleteFromReader(reader);
                        }
                    }
                }
                
                return null;
            }
        }

        public async Task<Athlete> GetAthleteByUserIdAsync(int userId)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                // First, find the user's AthleteId from the Users table
                using (var command = new NpgsqlCommand("SELECT athlete_id FROM users WHERE id = @userId", connection))
                {
                    command.Parameters.AddWithValue("@userId", userId);
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
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    INSERT INTO athletes (first_name, last_name, age, weight, height, sex, goal) 
                    VALUES (@firstName, @lastName, @age, @weight, @height, @sex, @goal)
                    RETURNING id";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@firstName", athlete.FirstName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@lastName", athlete.LastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@age", athlete.Age);
                    command.Parameters.AddWithValue("@weight", athlete.Weight);
                    command.Parameters.AddWithValue("@height", athlete.Height);
                    command.Parameters.AddWithValue("@sex", athlete.Sex);
                    command.Parameters.AddWithValue("@goal", athlete.Goal);
                    
                    int athleteId = Convert.ToInt32(await command.ExecuteScalarAsync());
                    athlete.Id = athleteId;
                    return athleteId;
                }
            }
        }

        public async Task<bool> UpdateAthleteAsync(Athlete athlete)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = @"
                    UPDATE athletes
                    SET first_name = @firstName, 
                        last_name = @lastName, 
                        age = @age, 
                        weight = @weight, 
                        height = @height, 
                        sex = @sex, 
                        goal = @goal
                    WHERE id = @id";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", athlete.Id);
                    command.Parameters.AddWithValue("@firstName", athlete.FirstName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@lastName", athlete.LastName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@age", athlete.Age);
                    command.Parameters.AddWithValue("@weight", athlete.Weight);
                    command.Parameters.AddWithValue("@height", athlete.Height);
                    command.Parameters.AddWithValue("@sex", athlete.Sex);
                    command.Parameters.AddWithValue("@goal", athlete.Goal);
                    
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        public async Task<bool> DeleteAthleteAsync(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                
                var query = "DELETE FROM athletes WHERE id = @id";
                
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    
                    var rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        private Athlete MapAthleteFromReader(NpgsqlDataReader reader)
        {
            return new Athlete
            {
                Id = Convert.ToInt32(reader["id"]),
                FirstName = reader["first_name"] as string ?? string.Empty,
                LastName = reader["last_name"] as string ?? string.Empty,
                Age = Convert.ToInt32(reader["age"]),
                Weight = Convert.ToInt32(reader["weight"]),
                Height = Convert.ToInt32(reader["height"]),
                Sex = reader["sex"] != DBNull.Value ? reader["sex"].ToString() : "Male",
                Goal = reader["goal"] != DBNull.Value ? reader["goal"].ToString() : "Maintenance"
            };
        }
    }
}