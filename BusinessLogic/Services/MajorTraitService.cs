using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class MajorTraitService : IMajorTraitService
    {
        private readonly IUnitOfWork _uow;

        public MajorTraitService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(Guid? majorId, Guid? traitId, int page = 1, int pageSize = 10)
        {
            var (data, total) = await _uow.MajorTraitRepository
                .GetPagedAsync(majorId, traitId, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<MajorTrait?> GetById(Guid id)
        {
            return await _uow.MajorTraitRepository.GetByIdAsync(id);
        }

        public async Task Create(MajorTrait entity)
        {
            entity.Id = Guid.NewGuid();

            await _uow.MajorTraitRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(MajorTrait entity)
        {
            await _uow.MajorTraitRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.MajorTraitRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
    }
}
