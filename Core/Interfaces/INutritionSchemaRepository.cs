using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Domain.Entities;

namespace Core.Interfaces
{
    public interface INutritionSchemaRepository
    {
        Task<List<NutritionSchema>> GetNutritionSchemasByAthleteIdAsync(int athleteId);
        Task<NutritionSchema> GetNutritionSchemaByIdAsync(int id);
        Task<NutritionSchema> CreateNutritionSchemaAsync(NutritionSchema schema);
        Task<NutritionSchema> UpdateNutritionSchemaAsync(NutritionSchema schema);
        Task<bool> DeleteNutritionSchemaAsync(int id);
    }
} 