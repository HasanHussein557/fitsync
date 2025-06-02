using System.Threading.Tasks;

namespace Core.Interfaces
{

    public interface INutritionSchemaGenerator
    {

        Task<string> GenerateNutritionSchemaAsync(int weight, int height, int age, string sex, 
            string goal, string dietaryPreferences, bool hasAllergies, string foodIntolerances, int durationWeeks = 4);
    }
} 