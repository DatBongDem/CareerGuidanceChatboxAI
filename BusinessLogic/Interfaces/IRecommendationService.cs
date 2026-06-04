using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IRecommendationService
    {
        Task<IEnumerable<Recommendation>> GetAllAsync();

        Task<Recommendation?> GetByIdAsync(Guid id);

        Task<Recommendation> CreateAsync(Recommendation model);
        Task<bool> UpdateAsync(Guid id, Recommendation model);

        Task<bool> DeleteAsync(Guid id);
    }
}
