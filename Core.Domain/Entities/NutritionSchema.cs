using System;
using System.Collections.Generic;

namespace Core.Domain.Entities
{
    public class NutritionSchema
    {
        public int Id { get; set; }
        public int AthleteId { get; set; }
        public string Name { get; set; }
        public DateTime CreatedDate { get; set; }
        public int DailyCalorieTarget { get; set; }
        public int ProteinTarget { get; set; }
        public int CarbTarget { get; set; }
        public int FatTarget { get; set; }
        public string Notes { get; set; }
        
        public List<MealPlan> MealPlans { get; set; } = new List<MealPlan>();
    }
    
    public class MealPlan
    {
        public int Id { get; set; }
        public int NutritionSchemaId { get; set; }
        public string Name { get; set; }
        public string Time { get; set; }
        public int Order { get; set; }
        
        public List<MealFood> Foods { get; set; } = new List<MealFood>();
    }
    
    public class MealFood
    {
        public int Id { get; set; }
        public int MealPlanId { get; set; }
        public int FoodId { get; set; }
        public decimal Quantity { get; set; }
        public string Notes { get; set; }
        
        public Food Food { get; set; }
    }
    
    public class Food
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Calories { get; set; }
        public decimal Protein { get; set; }
        public decimal Carbs { get; set; }
        public decimal Fat { get; set; }
        public decimal ServingSize { get; set; }
        public string ServingUnit { get; set; }
    }
} 