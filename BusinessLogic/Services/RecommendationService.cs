using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class RecommendationService : IRecommendationService
    {
        private readonly IUnitOfWork _uow;

        public RecommendationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<Recommendation>> GetAllAsync()
        {
            return await _uow.RecommendationRepository.GetAllAsync();
        }

        public async Task<Recommendation?> GetByIdAsync(Guid id)
        {
            return await _uow.RecommendationRepository.GetByIdAsync(id);
        }

        public async Task<Recommendation> CreateAsync(Recommendation model)
        {
            model.RecommendationId = Guid.NewGuid();
            model.CreatedAt = DateTime.UtcNow;

            await _uow.RecommendationRepository.AddAsync(model);
            await _uow.SaveAsync();

            return model;
        }

        // ✅ ✅ THÊM METHOD NÀY
        public async Task<bool> UpdateAsync(Guid id, Recommendation model)
        {
            var existing = await _uow.RecommendationRepository.GetByIdAsync(id);
            if (existing == null) return false;

            existing.ProfileId = model.ProfileId;
            existing.MajorId = model.MajorId;
            existing.MatchScore = model.MatchScore;
            existing.Reason = model.Reason;
            // ❌ KHÔNG sửa CreatedAt (giữ nguyên)

            await _uow.RecommendationRepository.UpdateAsync(existing);
            await _uow.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _uow.RecommendationRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.RecommendationRepository.DeleteAsync(id);
            await _uow.SaveAsync();

            return true;
        }
    }
}
