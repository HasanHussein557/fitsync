using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Core.Services;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;

namespace DAL.Repositories
{
    public class WorkoutSchemaRepository : IWorkoutSchemaRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<WorkoutSchemaRepository> _logger;
        private bool _tableChecked = false;
        private readonly JsonSerializerOptions _jsonOptions;

        public WorkoutSchemaRepository(string connectionString, ILogger<WorkoutSchemaRepository> logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
            
            // Configure JSON serialization options
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            
            // Add custom converters
            _jsonOptions.Converters.Add(new WorkoutJsonConverter());
            _jsonOptions.Converters.Add(new ExerciseJsonConverter());
        }

        private async Task EnsureTableExistsAsync()
        {
            if (_tableChecked) return;
            
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // First check if the table exists
                    string checkTableSql = @"
                        SELECT COUNT(*) 
                        FROM information_schema.tables 
                        WHERE table_schema = DATABASE() 
                        AND table_name = 'WorkoutSchemas'";
                    
                    using (var command = new MySqlCommand(checkTableSql, connection))
                    {
                        int tableExists = Convert.ToInt32(await command.ExecuteScalarAsync());
                        
                        if (tableExists == 0)
                        {
                            _logger?.LogInformation("WorkoutSchemas table doesn't exist. Creating it now.");
                            
                            // Table doesn't exist, create it
                            string createTableSql = @"
                                CREATE TABLE IF NOT EXISTS WorkoutSchemas (
                                    Id INT PRIMARY KEY AUTO_INCREMENT,
                                    AthleteId INT NOT NULL,
                                    Name VARCHAR(100),
                                    CreatedDate DATETIME NOT NULL,
                                    WorkoutsPerWeek INT NOT NULL,
                                    Goal VARCHAR(50),
                                    WorkoutsJson LONGTEXT,
                                    FOREIGN KEY (AthleteId) REFERENCES Athletes(Id) ON DELETE CASCADE
                                );
                                
                                CREATE INDEX IF NOT EXISTS IX_WorkoutSchemas_AthleteId ON WorkoutSchemas(AthleteId);
                            ";
                            
                            using (var createCommand = new MySqlCommand(createTableSql, connection))
                            {
                                await createCommand.ExecuteNonQueryAsync();
                                _logger?.LogInformation("WorkoutSchemas table created successfully.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error ensuring WorkoutSchemas table exists");
                throw new Exception("Failed to check or create WorkoutSchemas table. Please ensure the database is properly initialized.", ex);
            }
            
            _tableChecked = true;
        }

        public async Task<List<WorkoutSchema>> GetWorkoutSchemasByAthleteIdAsync(int athleteId)
        {
            await EnsureTableExistsAsync();
            
            var schemas = new List<WorkoutSchema>();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT * FROM WorkoutSchemas WHERE AthleteId = @AthleteId", connection))
                {
                    command.Parameters.AddWithValue("@AthleteId", athleteId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            schemas.Add(MapWorkoutSchemaFromReader((MySqlDataReader)reader));
                        }
                    }
                }
            }
            
            return schemas;
        }

        public async Task<WorkoutSchema> GetWorkoutSchemaByIdAsync(int id)
        {
            await EnsureTableExistsAsync();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("SELECT * FROM WorkoutSchemas WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return MapWorkoutSchemaFromReader((MySqlDataReader)reader);
                        }
                        return null;
                    }
                }
            }
        }

        public async Task<WorkoutSchema> CreateWorkoutSchemaAsync(WorkoutSchema workoutSchema)
        {
            await EnsureTableExistsAsync();
            
            workoutSchema.CreatedDate = DateTime.Now;
            
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    string sql = @"INSERT INTO WorkoutSchemas (AthleteId, Name, CreatedDate, WorkoutsPerWeek, Goal, WorkoutsJson) 
                                  VALUES (@AthleteId, @Name, @CreatedDate, @WorkoutsPerWeek, @Goal, @WorkoutsJson);
                                  SELECT LAST_INSERT_ID();";
                    
                    using (var command = new MySqlCommand(sql, connection))
                    {
                        AddWorkoutSchemaParameters(command, workoutSchema);
                        workoutSchema.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                        return workoutSchema;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating workout schema");
                throw;
            }
        }

        public async Task<WorkoutSchema> UpdateWorkoutSchemaAsync(WorkoutSchema workoutSchema)
        {
            await EnsureTableExistsAsync();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string sql = @"UPDATE WorkoutSchemas 
                              SET Name = @Name, 
                                  WorkoutsPerWeek = @WorkoutsPerWeek, 
                                  Goal = @Goal,
                                  WorkoutsJson = @WorkoutsJson
                              WHERE Id = @Id";
                
                using (var command = new MySqlCommand(sql, connection))
                {
                    AddWorkoutSchemaParameters(command, workoutSchema);
                    command.Parameters.AddWithValue("@Id", workoutSchema.Id);
                    await command.ExecuteNonQueryAsync();
                    return workoutSchema;
                }
            }
        }

        public async Task<bool> DeleteWorkoutSchemaAsync(int id)
        {
            await EnsureTableExistsAsync();
            
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var command = new MySqlCommand("DELETE FROM WorkoutSchemas WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

        private WorkoutSchema MapWorkoutSchemaFromReader(MySqlDataReader reader)
        {
            var workoutSchema = new WorkoutSchema
            {
                Id = Convert.ToInt32(reader["Id"]),
                AthleteId = Convert.ToInt32(reader["AthleteId"]),
                Name = reader["Name"].ToString(),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                WorkoutsPerWeek = Convert.ToInt32(reader["WorkoutsPerWeek"]),
                Goal = reader["Goal"].ToString(),
                Workouts = new List<Workout>() // Initialize with empty list
            };
            
            // Deserialize workouts from JSON
            try
            {
                if (!reader.IsDBNull(reader.GetOrdinal("WorkoutsJson")))
                {
                    string workoutsJson = reader["WorkoutsJson"].ToString();
                    if (!string.IsNullOrEmpty(workoutsJson))
                    {
                        var workouts = JsonSerializer.Deserialize<List<Workout>>(workoutsJson, _jsonOptions);
                        if (workouts != null)
                        {
                            workoutSchema.Workouts = workouts;
                        }
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "Error deserializing workouts JSON for schema {SchemaId}: {Json}", 
                    workoutSchema.Id, reader["WorkoutsJson"]);
                // Keep the empty list if deserialization fails
            }
            
            return workoutSchema;
        }

        private void AddWorkoutSchemaParameters(MySqlCommand command, WorkoutSchema workoutSchema)
        {
            command.Parameters.AddWithValue("@AthleteId", workoutSchema.AthleteId);
            command.Parameters.AddWithValue("@Name", workoutSchema.Name ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", workoutSchema.CreatedDate);
            command.Parameters.AddWithValue("@WorkoutsPerWeek", workoutSchema.WorkoutsPerWeek);
            command.Parameters.AddWithValue("@Goal", workoutSchema.Goal ?? (object)DBNull.Value);
            
            // Serialize workouts to JSON
            string workoutsJson = JsonSerializer.Serialize(workoutSchema.Workouts, _jsonOptions);
            command.Parameters.AddWithValue("@WorkoutsJson", workoutsJson ?? (object)DBNull.Value);
        }
    }
} 