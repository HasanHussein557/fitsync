using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace DAL.Repositories
{
    public class NutritionSchemaRepository : INutritionSchemaRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<NutritionSchemaRepository> _logger;

        public NutritionSchemaRepository(string connectionString, ILogger<NutritionSchemaRepository> logger = null)
        {
            _connectionString = connectionString;
            _logger = logger;
        }

        public async Task<List<NutritionSchema>> GetNutritionSchemasByAthleteIdAsync(int athleteId)
        {
            var schemas = new List<NutritionSchema>();
            
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    // Get nutrition schemas
                    var query = @"
                        SELECT * FROM nutrition_schemas 
                        WHERE athlete_id = @athleteId 
                        ORDER BY created_date DESC";
                    
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@athleteId", athleteId);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var schema = new NutritionSchema
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    AthleteId = Convert.ToInt32(reader["athlete_id"]),
                                    Name = reader["name"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["created_date"]),
                                    DailyCalorieTarget = reader["daily_calorie_target"] != DBNull.Value ? Convert.ToInt32(reader["daily_calorie_target"]) : 0,
                                    ProteinTarget = reader["protein_target"] != DBNull.Value ? Convert.ToInt32(reader["protein_target"]) : 0,
                                    CarbTarget = reader["carb_target"] != DBNull.Value ? Convert.ToInt32(reader["carb_target"]) : 0,
                                    FatTarget = reader["fat_target"] != DBNull.Value ? Convert.ToInt32(reader["fat_target"]) : 0,
                                    Notes = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : null,
                                    MealPlans = new List<MealPlan>()
                                };
                                
                                schemas.Add(schema);
                            }
                        }
                    }
                    
                    // For each schema, get meal plans and foods
                    foreach (var schema in schemas)
                    {
                        schema.MealPlans = await GetMealPlansForSchemaAsync(schema.Id, connection);
                    }
                }
                
                return schemas;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting nutrition schemas for athlete {AthleteId}", athleteId);
                throw;
            }
        }
        
        private async Task<List<MealPlan>> GetMealPlansForSchemaAsync(int schemaId, NpgsqlConnection connection)
        {
            var mealPlans = new List<MealPlan>();
            
            var query = @"
                SELECT * FROM meal_plans
                WHERE nutrition_schema_id = @schemaId
                ORDER BY meal_order";
                
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@schemaId", schemaId);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var mealPlan = new MealPlan
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            NutritionSchemaId = Convert.ToInt32(reader["nutrition_schema_id"]),
                            Name = reader["name"].ToString(),
                            Time = reader["time"] != DBNull.Value ? reader["time"].ToString() : null,
                            Order = Convert.ToInt32(reader["meal_order"]),
                            Foods = new List<MealFood>()
                        };
                        
                        mealPlans.Add(mealPlan);
                    }
                }
            }
            
            // Get foods for each meal plan
            foreach (var mealPlan in mealPlans)
            {
                mealPlan.Foods = await GetFoodsForMealPlanAsync(mealPlan.Id, connection);
            }
            
            return mealPlans;
        }
        
        private async Task<List<MealFood>> GetFoodsForMealPlanAsync(int mealPlanId, NpgsqlConnection connection)
        {
            var mealFoods = new List<MealFood>();
            
            var query = @"
                SELECT mf.*, f.*
                FROM meal_foods mf
                JOIN foods f ON mf.food_id = f.id
                WHERE mf.meal_plan_id = @mealPlanId";
                
            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@mealPlanId", mealPlanId);
                
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var mealFood = new MealFood
                        {
                            Id = Convert.ToInt32(reader["id"]),
                            MealPlanId = Convert.ToInt32(reader["meal_plan_id"]),
                            FoodId = Convert.ToInt32(reader["food_id"]),
                            Quantity = Convert.ToDecimal(reader["quantity"]),
                            Notes = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : null,
                            Food = new Food
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                Name = reader["name"].ToString(),
                                Calories = Convert.ToInt32(reader["calories"]),
                                Protein = Convert.ToDecimal(reader["protein"]),
                                Carbs = Convert.ToDecimal(reader["carbs"]),
                                Fat = Convert.ToDecimal(reader["fat"]),
                                ServingSize = Convert.ToDecimal(reader["serving_size"]),
                                ServingUnit = reader["serving_unit"].ToString()
                            }
                        };
                        
                        mealFoods.Add(mealFood);
                    }
                }
            }
            
            return mealFoods;
        }

        public async Task<NutritionSchema> GetNutritionSchemaByIdAsync(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "SELECT * FROM nutrition_schemas WHERE id = @id";
                    
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var schema = new NutritionSchema
                                {
                                    Id = Convert.ToInt32(reader["id"]),
                                    AthleteId = Convert.ToInt32(reader["athlete_id"]),
                                    Name = reader["name"].ToString(),
                                    CreatedDate = Convert.ToDateTime(reader["created_date"]),
                                    DailyCalorieTarget = reader["daily_calorie_target"] != DBNull.Value ? Convert.ToInt32(reader["daily_calorie_target"]) : 0,
                                    ProteinTarget = reader["protein_target"] != DBNull.Value ? Convert.ToInt32(reader["protein_target"]) : 0,
                                    CarbTarget = reader["carb_target"] != DBNull.Value ? Convert.ToInt32(reader["carb_target"]) : 0,
                                    FatTarget = reader["fat_target"] != DBNull.Value ? Convert.ToInt32(reader["fat_target"]) : 0,
                                    Notes = reader["notes"] != DBNull.Value ? reader["notes"].ToString() : null,
                                    MealPlans = new List<MealPlan>()
                                };
                                
                                // Get meal plans and foods
                                schema.MealPlans = await GetMealPlansForSchemaAsync(schema.Id, connection);
                                
                                return schema;
                            }
                        }
                    }
                    
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error getting nutrition schema {SchemaId}", id);
                throw;
            }
        }

        public async Task<NutritionSchema> CreateNutritionSchemaAsync(NutritionSchema schema)
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
                            // Insert nutrition schema
                            var query = @"
                                INSERT INTO nutrition_schemas (
                                    athlete_id, name, created_date, daily_calorie_target, 
                                    protein_target, carb_target, fat_target, notes
                                )
                                VALUES (
                                    @athleteId, @name, @createdDate, @dailyCalorieTarget, 
                                    @proteinTarget, @carbTarget, @fatTarget, @notes
                                )
                                RETURNING id";
                                
                            using (var command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@athleteId", schema.AthleteId);
                                command.Parameters.AddWithValue("@name", schema.Name ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@createdDate", schema.CreatedDate);
                                command.Parameters.AddWithValue("@dailyCalorieTarget", schema.DailyCalorieTarget > 0 ? schema.DailyCalorieTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@proteinTarget", schema.ProteinTarget > 0 ? schema.ProteinTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@carbTarget", schema.CarbTarget > 0 ? schema.CarbTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@fatTarget", schema.FatTarget > 0 ? schema.FatTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@notes", schema.Notes ?? (object)DBNull.Value);
                                
                                schema.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                            }
                            
                            // Insert meal plans and foods
                            if (schema.MealPlans != null)
                            {
                                for (int i = 0; i < schema.MealPlans.Count; i++)
                                {
                                    var mealPlan = schema.MealPlans[i];
                                    
                                    // Insert meal plan
                                    query = @"
                                        INSERT INTO meal_plans (nutrition_schema_id, name, time, meal_order)
                                        VALUES (@schemaId, @name, @time, @order)
                                        RETURNING id";
                                        
                                    using (var command = new NpgsqlCommand(query, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@schemaId", schema.Id);
                                        command.Parameters.AddWithValue("@name", mealPlan.Name);
                                        command.Parameters.AddWithValue("@time", mealPlan.Time ?? (object)DBNull.Value);
                                        command.Parameters.AddWithValue("@order", i);
                                        
                                        mealPlan.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                                    }
                                    
                                    // Insert foods
                                    if (mealPlan.Foods != null)
                                    {
                                        foreach (var mealFood in mealPlan.Foods)
                                        {
                                            // Check if food exists or create it
                                            int foodId = await GetOrCreateFoodAsync(mealFood.Food, connection, transaction);
                                            
                                            // Insert meal food
                                            query = @"
                                                INSERT INTO meal_foods (meal_plan_id, food_id, quantity, notes)
                                                VALUES (@mealPlanId, @foodId, @quantity, @notes)
                                                RETURNING id";
                                                
                                            using (var command = new NpgsqlCommand(query, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@mealPlanId", mealPlan.Id);
                                                command.Parameters.AddWithValue("@foodId", foodId);
                                                command.Parameters.AddWithValue("@quantity", mealFood.Quantity);
                                                command.Parameters.AddWithValue("@notes", mealFood.Notes ?? (object)DBNull.Value);
                                                
                                                mealFood.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
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
                            _logger?.LogError(ex, "Error creating nutrition schema");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating nutrition schema");
                throw;
            }
        }
        
        private async Task<int> GetOrCreateFoodAsync(Food food, NpgsqlConnection connection, NpgsqlTransaction transaction)
        {
            // Check if food exists
            var query = "SELECT id FROM foods WHERE LOWER(name) = LOWER(@name)";
            
            using (var command = new NpgsqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@name", food.Name);
                
                var result = await command.ExecuteScalarAsync();
                
                if (result != null)
                {
                    return Convert.ToInt32(result);
                }
            }
            
            // Food doesn't exist, create it
            query = @"
                INSERT INTO foods (name, calories, protein, carbs, fat, serving_size, serving_unit)
                VALUES (@name, @calories, @protein, @carbs, @fat, @servingSize, @servingUnit)
                RETURNING id";
                
            using (var command = new NpgsqlCommand(query, connection, transaction))
            {
                command.Parameters.AddWithValue("@name", food.Name);
                command.Parameters.AddWithValue("@calories", food.Calories);
                command.Parameters.AddWithValue("@protein", food.Protein);
                command.Parameters.AddWithValue("@carbs", food.Carbs);
                command.Parameters.AddWithValue("@fat", food.Fat);
                command.Parameters.AddWithValue("@servingSize", food.ServingSize);
                command.Parameters.AddWithValue("@servingUnit", food.ServingUnit);
                
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
        }

        public async Task<NutritionSchema> UpdateNutritionSchemaAsync(NutritionSchema schema)
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
                            // Update nutrition schema
                            var query = @"
                                UPDATE nutrition_schemas
                                SET name = @name, 
                                    daily_calorie_target = @dailyCalorieTarget, 
                                    protein_target = @proteinTarget, 
                                    carb_target = @carbTarget, 
                                    fat_target = @fatTarget,
                                    notes = @notes
                                WHERE id = @id";
                                
                            using (var command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@id", schema.Id);
                                command.Parameters.AddWithValue("@name", schema.Name ?? (object)DBNull.Value);
                                command.Parameters.AddWithValue("@dailyCalorieTarget", schema.DailyCalorieTarget > 0 ? schema.DailyCalorieTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@proteinTarget", schema.ProteinTarget > 0 ? schema.ProteinTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@carbTarget", schema.CarbTarget > 0 ? schema.CarbTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@fatTarget", schema.FatTarget > 0 ? schema.FatTarget : (object)DBNull.Value);
                                command.Parameters.AddWithValue("@notes", schema.Notes ?? (object)DBNull.Value);
                                
                                await command.ExecuteNonQueryAsync();
                            }
                            
                            // Delete existing meal plans and foods
                            query = "DELETE FROM meal_plans WHERE nutrition_schema_id = @schemaId";
                            
                            using (var command = new NpgsqlCommand(query, connection, transaction))
                            {
                                command.Parameters.AddWithValue("@schemaId", schema.Id);
                                await command.ExecuteNonQueryAsync();
                            }
                            
                            // Insert updated meal plans and foods
                            if (schema.MealPlans != null)
                            {
                                for (int i = 0; i < schema.MealPlans.Count; i++)
                                {
                                    var mealPlan = schema.MealPlans[i];
                                    
                                    // Insert meal plan
                                    query = @"
                                        INSERT INTO meal_plans (nutrition_schema_id, name, time, meal_order)
                                        VALUES (@schemaId, @name, @time, @order)
                                        RETURNING id";
                                        
                                    using (var command = new NpgsqlCommand(query, connection, transaction))
                                    {
                                        command.Parameters.AddWithValue("@schemaId", schema.Id);
                                        command.Parameters.AddWithValue("@name", mealPlan.Name);
                                        command.Parameters.AddWithValue("@time", mealPlan.Time ?? (object)DBNull.Value);
                                        command.Parameters.AddWithValue("@order", i);
                                        
                                        mealPlan.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
                                    }
                                    
                                    // Insert foods
                                    if (mealPlan.Foods != null)
                                    {
                                        foreach (var mealFood in mealPlan.Foods)
                                        {
                                            // Check if food exists or create it
                                            int foodId = await GetOrCreateFoodAsync(mealFood.Food, connection, transaction);
                                            
                                            // Insert meal food
                                            query = @"
                                                INSERT INTO meal_foods (meal_plan_id, food_id, quantity, notes)
                                                VALUES (@mealPlanId, @foodId, @quantity, @notes)
                                                RETURNING id";
                                                
                                            using (var command = new NpgsqlCommand(query, connection, transaction))
                                            {
                                                command.Parameters.AddWithValue("@mealPlanId", mealPlan.Id);
                                                command.Parameters.AddWithValue("@foodId", foodId);
                                                command.Parameters.AddWithValue("@quantity", mealFood.Quantity);
                                                command.Parameters.AddWithValue("@notes", mealFood.Notes ?? (object)DBNull.Value);
                                                
                                                mealFood.Id = Convert.ToInt32(await command.ExecuteScalarAsync());
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
                            _logger?.LogError(ex, "Error updating nutrition schema");
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error updating nutrition schema");
                throw;
            }
        }

        public async Task<bool> DeleteNutritionSchemaAsync(int id)
        {
            try
            {
                using (var connection = new NpgsqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    
                    var query = "DELETE FROM nutrition_schemas WHERE id = @id";
                    
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
                _logger?.LogError(ex, "Error deleting nutrition schema {SchemaId}", id);
                throw;
            }
        }
    }
} 