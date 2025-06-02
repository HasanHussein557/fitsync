using System.Threading.Tasks;

namespace Core.Interfaces
{
    public interface IWorkoutSchemaGenerator
    {
        Task<string> GenerateWorkoutSchemaAsync(int weight, int height, int age, string sex, string goal, int workoutsPerWeek);
    }
}