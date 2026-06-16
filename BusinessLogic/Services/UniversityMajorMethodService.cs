using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class UniversityMajorMethodService : IUniversityMajorMethodService
    {
        private readonly IUnitOfWork _uow;

        public UniversityMajorMethodService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            int page = 1,
            int pageSize = 10)
        {
            var (data, total) = await _uow.UniversityMajorMethodRepository
                .GetPagedAsync(universityId, majorId, methodId, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<UniversityMajorMethod?> GetById(Guid id)
        {
            return await _uow.UniversityMajorMethodRepository.GetByIdAsync(id);
        }

        public async Task Create(UniversityMajorMethod entity)
        {
            entity.Id = Guid.NewGuid();

            await _uow.UniversityMajorMethodRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(UniversityMajorMethod entity)
        {
            await _uow.UniversityMajorMethodRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.UniversityMajorMethodRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
    }
}
