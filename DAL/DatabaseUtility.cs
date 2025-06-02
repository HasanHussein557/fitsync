using System;
using System.IO;
using System.Threading.Tasks;
using Npgsql;
using Microsoft.Extensions.Logging;

namespace DAL
{
    public class DatabaseUtility
    {
        private readonly string _connectionString;
        private readonly ILogger<DatabaseUtility> _logger;

        public DatabaseUtility(string connectionString, ILogger<DatabaseUtility> logger)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task ExecuteSqlScriptAsync(string scriptPath)
        {
            try
            {
                _logger.LogInformation("Reading SQL script from {path}...", scriptPath);
                string sqlScript = await File.ReadAllTextAsync(scriptPath);
                
                await ExecuteSqlAsync(sqlScript);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL script from {path}", scriptPath);
                throw;
            }
        }

        public async Task ExecuteSqlAsync(string sqlScript)
        {
            try
            {
                _logger.LogInformation("Connecting to PostgreSQL database...");
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger.LogInformation("Connection successful!");
                    
                    // Split script by ; to execute each statement separately
                    string[] commands = sqlScript.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string command in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(command))
                        {
                            using (var cmd = new NpgsqlCommand(command, connection))
                            {
                                try
                                {
                                    _logger.LogInformation("Executing SQL command...");
                                    await cmd.ExecuteNonQueryAsync();
                                    _logger.LogInformation("Command executed successfully.");
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, "Error executing command: {command}", command.Trim());
                                    throw;
                                }
                            }
                        }
                    }
                    
                    _logger.LogInformation("All SQL commands executed.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing SQL script");
                throw;
            }
        }

        // This is the main SQL script for resetting tables
        public static string GetResetTablesSql()
        {
            return @"
                -- Drop existing tables in correct order
                DROP TABLE IF EXISTS meal_foods;
                DROP TABLE IF EXISTS meal_plans;
                DROP TABLE IF EXISTS nutrition_schemas;
                DROP TABLE IF EXISTS workout_exercises;
                DROP TABLE IF EXISTS workouts;
                DROP TABLE IF EXISTS workout_schemas;
                DROP TABLE IF EXISTS exercises;
                DROP TABLE IF EXISTS user_roles;
                DROP TABLE IF EXISTS users;
                DROP TABLE IF EXISTS athletes;
                DROP TABLE IF EXISTS foods;

                -- Now recreate tables
                -- Athletes table
                CREATE TABLE IF NOT EXISTS athletes (
                    id SERIAL PRIMARY KEY,
                    first_name VARCHAR(50),
                    last_name VARCHAR(50),
                    age INTEGER NOT NULL,
                    weight DECIMAL(5,2) NOT NULL,
                    height DECIMAL(5,2) NOT NULL,
                    sex VARCHAR(10) NOT NULL,
                    goal VARCHAR(50) NOT NULL
                );

                -- Users table
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(50) NOT NULL UNIQUE,
                    email VARCHAR(100) NOT NULL UNIQUE,
                    password_hash VARCHAR(255) NOT NULL,
                    salt VARCHAR(255) NOT NULL,
                    athlete_id INTEGER REFERENCES athletes(id) ON DELETE CASCADE,
                    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    last_login_date TIMESTAMP
                );

                -- User roles table
                CREATE TABLE IF NOT EXISTS user_roles (
                    user_id INTEGER NOT NULL REFERENCES users(id) ON DELETE CASCADE,
                    role VARCHAR(50) NOT NULL,
                    PRIMARY KEY (user_id, role)
                );

                -- Exercises table
                CREATE TABLE IF NOT EXISTS exercises (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    category VARCHAR(50),
                    primary_muscle_group VARCHAR(50),
                    description TEXT
                );

                -- Workout schemas table
                CREATE TABLE IF NOT EXISTS workout_schemas (
                    id SERIAL PRIMARY KEY,
                    athlete_id INTEGER NOT NULL REFERENCES athletes(id) ON DELETE CASCADE,
                    name VARCHAR(100),
                    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    workouts_per_week INTEGER NOT NULL,
                    goal VARCHAR(50)
                );

                -- Workouts table
                CREATE TABLE IF NOT EXISTS workouts (
                    id SERIAL PRIMARY KEY,
                    workout_schema_id INTEGER NOT NULL REFERENCES workout_schemas(id) ON DELETE CASCADE,
                    name VARCHAR(100) NOT NULL,
                    day_of_week INTEGER NOT NULL,
                    workout_order INTEGER NOT NULL DEFAULT 0,
                    notes TEXT
                );

                -- Workout exercises junction table
                CREATE TABLE IF NOT EXISTS workout_exercises (
                    id SERIAL PRIMARY KEY,
                    workout_id INTEGER NOT NULL REFERENCES workouts(id) ON DELETE CASCADE,
                    exercise_id INTEGER NOT NULL REFERENCES exercises(id) ON DELETE CASCADE,
                    sets INTEGER NOT NULL DEFAULT 3,
                    reps VARCHAR(20) NOT NULL DEFAULT '10-12',
                    weight VARCHAR(20),
                    duration VARCHAR(20),
                    rest VARCHAR(20) NOT NULL DEFAULT '60s',
                    exercise_order INTEGER NOT NULL DEFAULT 0,
                    notes TEXT
                );

                -- Nutrition schemas table
                CREATE TABLE IF NOT EXISTS nutrition_schemas (
                    id SERIAL PRIMARY KEY,
                    athlete_id INTEGER NOT NULL REFERENCES athletes(id) ON DELETE CASCADE,
                    name VARCHAR(100),
                    created_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
                    daily_calorie_target INTEGER,
                    protein_target INTEGER,
                    carb_target INTEGER,
                    fat_target INTEGER,
                    notes TEXT
                );

                -- Meal plans table
                CREATE TABLE IF NOT EXISTS meal_plans (
                    id SERIAL PRIMARY KEY,
                    nutrition_schema_id INTEGER NOT NULL REFERENCES nutrition_schemas(id) ON DELETE CASCADE,
                    name VARCHAR(50) NOT NULL,
                    time VARCHAR(20),
                    meal_order INTEGER NOT NULL DEFAULT 0
                );

                -- Foods table
                CREATE TABLE IF NOT EXISTS foods (
                    id SERIAL PRIMARY KEY,
                    name VARCHAR(100) NOT NULL,
                    calories INTEGER NOT NULL,
                    protein DECIMAL(5,2) NOT NULL,
                    carbs DECIMAL(5,2) NOT NULL,
                    fat DECIMAL(5,2) NOT NULL,
                    serving_size DECIMAL(5,2) NOT NULL,
                    serving_unit VARCHAR(20) NOT NULL
                );

                -- Meal foods junction table
                CREATE TABLE IF NOT EXISTS meal_foods (
                    id SERIAL PRIMARY KEY,
                    meal_plan_id INTEGER NOT NULL REFERENCES meal_plans(id) ON DELETE CASCADE,
                    food_id INTEGER NOT NULL REFERENCES foods(id) ON DELETE CASCADE,
                    quantity DECIMAL(5,2) NOT NULL,
                    notes TEXT
                );

                -- Create indexes for performance
                CREATE INDEX IF NOT EXISTS idx_users_athlete_id ON users(athlete_id);
                CREATE INDEX IF NOT EXISTS idx_workout_schemas_athlete_id ON workout_schemas(athlete_id);
                CREATE INDEX IF NOT EXISTS idx_workouts_schema_id ON workouts(workout_schema_id);
                CREATE INDEX IF NOT EXISTS idx_workout_exercises_workout_id ON workout_exercises(workout_id);
                CREATE INDEX IF NOT EXISTS idx_workout_exercises_exercise_id ON workout_exercises(exercise_id);
                CREATE INDEX IF NOT EXISTS idx_nutrition_schemas_athlete_id ON nutrition_schemas(athlete_id);
                CREATE INDEX IF NOT EXISTS idx_meal_plans_schema_id ON meal_plans(nutrition_schema_id);
                CREATE INDEX IF NOT EXISTS idx_meal_foods_meal_id ON meal_foods(meal_plan_id);
                CREATE INDEX IF NOT EXISTS idx_meal_foods_food_id ON meal_foods(food_id);";
        }
    }
} 