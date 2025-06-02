using System.Text.Json.Serialization;

namespace Core.Domain.Entities;

public class Exercise
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Sets { get; set; }
    public string Reps { get; set; }
    public string Rest { get; set; }
    public string Category { get; set; }
    public string PrimaryMuscleGroup { get; set; }
    public string Description { get; set; }
    public Intensity Intensity { get; set; }
}