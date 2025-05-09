using System;
using System.IO;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
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
                _logger.LogInformation("Connecting to MySQL database...");
                using (var connection = new MySqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    _logger.LogInformation("Connection successful!");
                    
                    // Split script by ; to execute each statement separately
                    string[] commands = sqlScript.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                    
                    foreach (string command in commands)
                    {
                        if (!string.IsNullOrWhiteSpace(command))
                        {
                            using (var cmd = new MySqlCommand(command, connection))
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
            -- Drop tables in reverse order of dependency
            DROP TABLE IF EXISTS WorkoutSchemas;
            DROP TABLE IF EXISTS Users;
            DROP TABLE IF EXISTS Athletes;

            -- Create Athletes table
            CREATE TABLE IF NOT EXISTS Athletes (
                Id INT NOT NULL AUTO_INCREMENT,
                FirstName VARCHAR(50) NULL,
                LastName VARCHAR(50) NULL,
                Age INT NOT NULL,
                Weight DECIMAL(5,2) NOT NULL,
                Height DECIMAL(5,2) NOT NULL,
                Sex VARCHAR(10) NOT NULL,
                Goal VARCHAR(50) NOT NULL,
                PRIMARY KEY (Id)
            );

            -- Create Users table
            CREATE TABLE IF NOT EXISTS Users (
                Id INT NOT NULL AUTO_INCREMENT,
                Username VARCHAR(50) NOT NULL,
                Email VARCHAR(100) NOT NULL,
                PasswordHash VARCHAR(255) NOT NULL,
                Salt VARCHAR(255) NOT NULL,
                AthleteId INT NULL,
                CreatedDate DATETIME NOT NULL,
                LastLoginDate DATETIME NULL,
                PRIMARY KEY (Id),
                UNIQUE INDEX UX_Users_Username (Username),
                UNIQUE INDEX UX_Users_Email (Email),
                CONSTRAINT FK_Users_Athletes FOREIGN KEY (AthleteId) 
                    REFERENCES Athletes (Id) ON DELETE SET NULL
            );
            
            -- Create WorkoutSchemas table
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
            
            -- Create index for faster lookups by AthleteId
            CREATE INDEX IX_WorkoutSchemas_AthleteId ON WorkoutSchemas(AthleteId);

            -- Insert an admin user with athlete profile
            INSERT INTO Athletes (FirstName, LastName, Age, Weight, Height, Sex, Goal)
            VALUES ('Admin', 'User', 30, 70.0, 175.0, 'Male', 'Maintenance');

            INSERT INTO Users (Username, Email, PasswordHash, Salt, AthleteId, CreatedDate)
            VALUES ('admin', 'admin@fitsync.com', 
                    'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- password: admin123
                    'admin_salt',
                    1,
                    NOW());
            ";
        }
    }
} 