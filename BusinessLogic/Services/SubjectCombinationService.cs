using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class SubjectCombinationService : ISubjectCombinationService
    {
        private readonly IUnitOfWork _uow;

        public SubjectCombinationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> GetAll(string search, int page = 1, int pageSize = 10)
        {
            var (data, total) = await _uow.SubjectCombinationRepository
                .GetPagedAsync(search, page, pageSize);

            return new
            {
                total,
                page,
                pageSize,
                data
            };
        }

        public async Task<SubjectCombination?> GetById(Guid id)
        {
            return await _uow.SubjectCombinationRepository.GetByIdAsync(id);
        }

        public async Task Create(SubjectCombination entity)
        {
            entity.CombinationId = Guid.NewGuid();

            await _uow.SubjectCombinationRepository.AddAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Update(SubjectCombination entity)
        {
            await _uow.SubjectCombinationRepository.UpdateAsync(entity);
            await _uow.SaveAsync();
        }

        public async Task Delete(Guid id)
        {
            await _uow.SubjectCombinationRepository.DeleteAsync(id);
            await _uow.SaveAsync();
        }
    }
}