using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class UniversityService : IUniversityService
    {
        private readonly IUnitOfWork _uow;

        public UniversityService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(string search, int page = 1, int pageSize = 10)
        {
            var (data, total) = await _uow.UniversityRepository
                .GetPagedAsync(search, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<University?> GetById(Guid id)
        {
            return await _uow.UniversityRepository.GetByIdAsync(id);
        }

        public async Task Create(University entity)
        {
            entity.UniversityId = Guid.NewGuid();

            await _uow.UniversityRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(University entity)
        {
            await _uow.UniversityRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.UniversityRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
    }
}