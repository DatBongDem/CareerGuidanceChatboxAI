using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class UniversityMajorAdmissionService : IUniversityMajorAdmissionService
    {
        private readonly IUnitOfWork _uow;

        public UniversityMajorAdmissionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            Guid? combinationId,
            int? year,
            double? minScore,
            double? maxScore,
            int page = 1,
            int pageSize = 10)
        {
            var (data, total) = await _uow.UniversityMajorAdmissionRepository
                .GetPagedAsync(universityId, majorId, methodId, combinationId, year, minScore, maxScore, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<UniversityMajorAdmission?> GetById(Guid id)
        {
            return await _uow.UniversityMajorAdmissionRepository.GetByIdAsync(id);
        }

        public async Task Create(UniversityMajorAdmission entity)
        {
            entity.Id = Guid.NewGuid();

            await _uow.UniversityMajorAdmissionRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(UniversityMajorAdmission entity)
        {
            await _uow.UniversityMajorAdmissionRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.UniversityMajorAdmissionRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
    }
}
