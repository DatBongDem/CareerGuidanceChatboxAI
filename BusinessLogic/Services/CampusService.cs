using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class CampusService : ICampusService
    {
        private readonly IUnitOfWork _uow;

        public CampusService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(Guid? universityId, string search, int page = 1, int pageSize = 10)
        {
            var (data, total) = await _uow.CampusRepository
                .GetPagedAsync(universityId, search, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<Campus?> GetById(Guid id)
        {
            return await _uow.CampusRepository.GetByIdAsync(id);
        }

        public async Task Create(Campus entity)
        {
            entity.CampusId = Guid.NewGuid();

            await _uow.CampusRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(Campus entity)
        {
            await _uow.CampusRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.CampusRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
    }
}