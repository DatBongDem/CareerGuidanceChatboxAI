using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class TraitService : ITraitService
    {
        private readonly IUnitOfWork _uow;

        public TraitService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(string search, int page = 1, int pageSize = 10)
        {
            var (data, total) = await _uow.TraitRepository
                .GetPagedAsync(search, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<Trait?> GetById(Guid id)
        {
            return await _uow.TraitRepository.GetByIdAsync(id);
        }

        public async Task Create(Trait entity)
        {
            entity.TraitId = Guid.NewGuid();

            await _uow.TraitRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(Trait entity)
        {
            await _uow.TraitRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.TraitRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
    }
}