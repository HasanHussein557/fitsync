using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Core.Domain.Entities
{
    public class WorkoutSchema
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
        
        public int AthleteId { get; set; }    
        
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        public int WorkoutsPerWeek { get; set; }
        
        public string Goal { get; set; } = "Maintenance";
        
        public List<Workout> Workouts { get; set; } = new List<Workout>();
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public class Workout
    {
        public int Id { get; set; }
        
        public string Name { get; set; } = string.Empty;
        
        public int DayOfWeek { get; set; }
        
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
