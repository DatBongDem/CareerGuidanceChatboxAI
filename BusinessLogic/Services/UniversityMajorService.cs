using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class UniversityMajorService : IUniversityMajorService
    {
        private readonly IUnitOfWork _uow;

        public UniversityMajorService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(string search, int page = 1, int pageSize = 10)
        {
            var (data, total) = await _uow.UniversityMajorRepository
                .GetPagedAsync(search, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<UniversityMajor?> GetById(Guid id)
        {
            return await _uow.UniversityMajorRepository.GetByIdAsync(id);
        }

        public async Task<object> GetByUniversity(Guid universityId, int page, int pageSize)
        {
            var (data, total) = await _uow.UniversityMajorRepository
                .GetByUniversityAsync(universityId, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<object> GetByMajor(Guid majorId, int page, int pageSize)
        {
            var (data, total) = await _uow.UniversityMajorRepository
                .GetByMajorAsync(majorId, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task Create(UniversityMajor entity)
        {
            entity.Id = Guid.NewGuid();
            entity.CreatedAt = DateTime.UtcNow;

            await _uow.UniversityMajorRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(UniversityMajor entity)
        {
            await _uow.UniversityMajorRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.UniversityMajorRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
        public async Task<object> Filter(
    Guid? universityId,
    Guid? majorId,
    int? year,
    double? minScore,
    double? maxScore,
    int page = 1,
    int pageSize = 10)
        {
            var (data, total) = await _uow.UniversityMajorRepository
                .FilterAsync(universityId, majorId, year, minScore, maxScore, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }
    }
}