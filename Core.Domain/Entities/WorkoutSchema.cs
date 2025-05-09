using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Core.Domain.Entities
{
    public class WorkoutSchema
    {
        public int Id { get; set; }
        
        public int AthleteId { get; set; }
        
        public Athlete Athlete { get; set; }
        
        public string Name { get; set; }
        
        public DateTime CreatedDate { get; set; }
        
        public int WorkoutsPerWeek { get; set; }
        
        public string Goal { get; set; }
        
        public List<Workout> Workouts { get; set; } = new List<Workout>();
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public class Workout
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public int DayOfWeek { get; set; }
        
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}
