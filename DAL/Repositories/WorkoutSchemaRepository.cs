using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DAL.Repositories
{
    public class WorkoutSchemaRepository : IWorkoutSchemaRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<WorkoutSchemaRepository> _logger;

        public WorkoutSchemaRepository(string connectionString, ILogger<WorkoutSchemaRepository> logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<List<WorkoutSchema>> GetWorkoutSchemasByAthleteIdAsync(int athleteId)
        {
            var workoutSchemas = new List<WorkoutSchema>();
            
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Get workout schemas
                    var query = @"
                        SELECT * FROM workout_schemas 
                        WHERE athlete_id = @athleteId 
                        ORDER BY created_date DESC";
                    
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@athleteId", athleteId);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var schema = new WorkoutSchema
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    AthleteId = Convert.ToInt32(reader["athlete_id"]),
                                    Name = reader["name"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["created_date"]),
                                    WorkoutsPerWeek = Convert.ToInt32(reader["workouts_per_week"]),
                                    Goal = reader["goal"].ToString(),
                                    Workouts = new List<Workout>()
                                };
                                
                                workoutSchemas.Add(schema);
                            }
                        }
                    }
                    
                    // For each schema, get workouts and exercises
                    foreach (var schema in workoutSchemas)
                    {
                        schema.Workouts = await GetWorkoutsForSchemaAsync(schema.Id, connection);
                    }
                }
                
                return workoutSchemas;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting workout schemas for athlete {AthleteId}", athleteId);
                throw;
            }
        }
        
        private async Task<List<Workout>> GetWorkoutsForSchemaAsync(int schemaId, NpgsqlConnection connection)
        {
            var workouts = new List<Workout>();
            
            var query = @"
                SELECT * FROM workouts
                WHERE workout_schema_id = @schemaId
                ORDER BY day_of_week";
                
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@schemaId", schemaId);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var workout = new Workout
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            Name = reader["name"].ToString(),
                            DayOfWeek = Convert.ToInt32(reader["day_of_week"]),
                            Exercises = new List<Exercise>()
                        };
                        
                        workouts.Add(workout);
                    }
                }
            }
            
            // Get exercises for each workout
            foreach (var workout in workouts)
            {
                workout.Exercises = await GetExercisesForWorkoutAsync(workout.Id, connection);
            }
            
            return workouts;
        }
        
        private async Task<List<Exercise>> GetExercisesForWorkoutAsync(int workoutId, NpgsqlConnection connection)
        {
            var exercises = new List<Exercise>();
            
            var query = @"
                SELECT we.*, e.name as exercise_name, e.category, e.primary_muscle_group, e.description
                FROM workout_exercises we
                JOIN exercises e ON we.exercise_id = e.id
                WHERE we.workout_id = @workoutId
                ORDER BY we.exercise_order";
                
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@workoutId", workoutId);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var exercise = new Exercise
                        {
                            Id = Convert.ToInt32(reader["exercise_id"]),
                            Name = reader["exercise_name"].ToString(),
                            Sets = Convert.ToInt32(reader["sets"]),
                            Reps = reader["reps"].ToString(),
                            Rest = reader["rest"].ToString(),
                            Category = reader["category"].ToString(),
                            PrimaryMuscleGroup = reader["primary_muscle_group"].ToString(),
                            Description = reader["description"].ToString()
                        };
                        
                        exercises.Add(exercise);
                    }
                }
            }
            
            return exercises;
        }

        public async Task<WorkoutSchema> GetWorkoutSchemaByIdAsync(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    WorkoutSchema schema = null;
                    
                    var query = "SELECT * FROM workout_schemas WHERE id = @id";
                    
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                schema = new WorkoutSchema
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    AthleteId = Convert.ToInt32(reader["athlete_id"]),
                                    Name = reader["name"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["created_date"]),
                                    WorkoutsPerWeek = Convert.ToInt32(reader["workouts_per_week"]),
                                    Goal = reader["goal"].ToString(),
                                    Workouts = new List<Workout>()
                                };
                            }
                        }
                        // Reader is now closed
                    }
                    
                    // Now we can safely call GetWorkoutsForSchemaAsync
                    if (schema != null)
                    {
                        schema.Workouts = await GetWorkoutsForSchemaAsync(schema.Id, connection);
                    }
                    
                    return schema;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting workout schema {SchemaId}", id);
                throw;
            }
        }

        public async Task<WorkoutSchema> CreateWorkoutSchemaAsync(WorkoutSchema schema)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Start a transaction
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            // Insert workout schema
                            var query = @"
                                INSERT INTO workout_schemas (athlete_id, name, created_date, workouts_per_week, goal)
                                VALUES (@athleteId, @name, @createdDate, @workoutsPerWeek, @goal)
                                RETURNING id";
                                
                            using (var command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@athleteId", schema.AthleteId);
                                command.Parameters.AddWithValue("@name", schema.Name ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@createdDate", schema.CreatedDate);
                                command.Parameters.AddWithValue("@workoutsPerWeek", schema.WorkoutsPerWeek);
                                command.Parameters.AddWithValue("@goal", schema.Goal ?? (object)DBNull.Value);
                                
                                schema.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                            }
                            
                            // Insert workouts and exercises
                            if (schema.Workouts != null)
                            {
                                for (int i = 0; i < schema.Workouts.Count; i++)
                                {
                                    var workout = schema.Workouts[i];
                                    
                                    // Insert workout
                                    query = @"
                                        INSERT INTO workouts (workout_schema_id, name, day_of_week, workout_order)
                                        VALUES (@schemaId, @name, @dayOfWeek, @order)
                                        RETURNING id";
                                        
                                    using (var command = new NpgsqlCommand(query, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@schemaId", schema.Id);
                                        command.Parameters.AddWithValue("@name", workout.Name ?? $"Workout {i+1}");
                                        command.Parameters.AddWithValue("@dayOfWeek", workout.DayOfWeek);
                                        command.Parameters.AddWithValue("@order", i);
                                        
                                        workout.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                                    }
                                    
                                    // Insert exercises
                                    if (workout.Exercises != null)
                                    {
                                        for (int j = 0; j < workout.Exercises.Count; j++)
                                        {
                                            var exercise = workout.Exercises[j];
                                            
                                            // Check if exercise exists or create it
                                            int exerciseId = await GetOrCreateExerciseAsync(exercise, connection, transaction);
                                            
                                            // Insert workout exercise
                                            query = @"
                                                INSERT INTO workout_exercises (workout_id, exercise_id, sets, reps, rest, exercise_order)
                                                VALUES (@workoutId, @exerciseId, @sets, @reps, @rest, @order)";
                                                
                                            using (var command = new NpgsqlCommand(query, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@workoutId", workout.Id);
                                                command.Parameters.AddWithValue("@exerciseId", exerciseId);
                                                command.Parameters.AddWithValue("@sets", exercise.Sets);
                                                command.Parameters.AddWithValue("@reps", exercise.Reps ?? "10-12");
                                                command.Parameters.AddWithValue("@rest", exercise.Rest ?? "60s");
                                                command.Parameters.AddWithValue("@order", j);
                                                
                                                await command.ExecuteNonQueryAsync();
                                            }
                                        }
                                    }
                                }
                            }
                            
                            await transaction.CommitAsync();
                            return schema;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger?.LogError(ex, "Error creating workout schema");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating workout schema");
                throw;
            }
        }
        
        private async Task<int> GetOrCreateExerciseAsync(Exercise exercise, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            // Check if exercise exists
            var query = "SELECT id FROM exercises WHERE LOWER(name) = LOWER(@name)";
            
            using (var command = new NpgsqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@name", exercise.Name);
                
                var result = await command.ExecuteScalarAsync();
                
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }
            
            // Exercise doesn't exist, create it
            query = @"
                INSERT INTO exercises (name, category, primary_muscle_group, description)
                VALUES (@name, @category, @primaryMuscleGroup, @description)
                RETURNING id";
                
            using (var command = new NpgsqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@name", exercise.Name);
                command.Parameters.AddWithValue("@category", exercise.Category ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@primaryMuscleGroup", exercise.PrimaryMuscleGroup ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@description", exercise.Description ?? (object)DBNull.Value);
                
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public async Task<WorkoutSchema> UpdateWorkoutSchemaAsync(WorkoutSchema schema)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Start a transaction
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        try
                        {
                            // Update workout schema
                            var query = @"
                                UPDATE workout_schemas
                                SET name = @name, goal = @goal, workouts_per_week = @workoutsPerWeek
                                WHERE id = @id";
                                
                            using (var command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", schema.Id);
                                command.Parameters.AddWithValue("@name", schema.Name ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@goal", schema.Goal ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@workoutsPerWeek", schema.WorkoutsPerWeek);
                                
                                await command.ExecuteNonQueryAsync();
                            }
                            
                            // Delete existing workouts and exercises
                            query = "DELETE FROM workouts WHERE workout_schema_id = @schemaId";
                            
                            using (var command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@schemaId", schema.Id);
                                await command.ExecuteNonQueryAsync();
                            }
                            
                            // Insert updated workouts and exercises
                            if (schema.Workouts != null)
                            {
                                for (int i = 0; i < schema.Workouts.Count; i++)
                                {
                                    var workout = schema.Workouts[i];
                                    
                                    // Insert workout
                                    query = @"
                                        INSERT INTO workouts (workout_schema_id, name, day_of_week, workout_order)
                                        VALUES (@schemaId, @name, @dayOfWeek, @order)
                                        RETURNING id";
                                        
                                    using (var command = new NpgsqlCommand(query, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@schemaId", schema.Id);
                                        command.Parameters.AddWithValue("@name", workout.Name ?? $"Workout {i+1}");
                                        command.Parameters.AddWithValue("@dayOfWeek", workout.DayOfWeek);
                                        command.Parameters.AddWithValue("@order", i);
                                        
                                        workout.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                                    }
                                    
                                    // Insert exercises
                                    if (workout.Exercises != null)
                                    {
                                        for (int j = 0; j < workout.Exercises.Count; j++)
                                        {
                                            var exercise = workout.Exercises[j];
                                            
                                            // Check if exercise exists or create it
                                            int exerciseId = await GetOrCreateExerciseAsync(exercise, connection, transaction);
                                            
                                            // Insert workout exercise
                                            query = @"
                                                INSERT INTO workout_exercises (workout_id, exercise_id, sets, reps, rest, exercise_order)
                                                VALUES (@workoutId, @exerciseId, @sets, @reps, @rest, @order)";
                                                
                                            using (var command = new NpgsqlCommand(query, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@workoutId", workout.Id);
                                                command.Parameters.AddWithValue("@exerciseId", exerciseId);
                                                command.Parameters.AddWithValue("@sets", exercise.Sets);
                                                command.Parameters.AddWithValue("@reps", exercise.Reps ?? "10-12");
                                                command.Parameters.AddWithValue("@rest", exercise.Rest ?? "60s");
                                                command.Parameters.AddWithValue("@order", j);
                                                
                                                await command.ExecuteNonQueryAsync();
                                            }
                                        }
                                    }
                                }
                            }
                            
                            await transaction.CommitAsync();
                            return schema;
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            _logger?.LogError(ex, "Error updating workout schema");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating workout schema");
                throw;
            }
        }

        public async Task<bool> DeleteWorkoutSchemaAsync(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "DELETE FROM workout_schemas WHERE id = @id";
                    
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        
                        var rowsAffected = await command.ExecuteNonQueryAsync();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error deleting workout schema {SchemaId}", id);
                throw;
            }
        }
    }
} 