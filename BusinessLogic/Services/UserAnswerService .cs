using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class UserAnswerService : IUserAnswerService
    {
        private readonly IUnitOfWork _uow;

        public UserAnswerService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<UserAnswer>> GetAllAsync()
        {
            return await _uow.UserAnswerRepository.GetAllAsync();
        }

        public async Task<UserAnswer?> GetByIdAsync(Guid id)
        {
            return await _uow.UserAnswerRepository.GetByIdAsync(id);
        }

        public async Task<UserAnswer> CreateAsync(UserAnswer model)
        {
            model.UserAnswerId = Guid.NewGuid();

            await _uow.UserAnswerRepository.AddAsync(model);
            await _uow.SaveAsync();

            return model;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _uow.UserAnswerRepository.GetByIdAsync(id);
            if (existing == null) return false;

            await _uow.UserAnswerRepository.DeleteAsync(id);
            await _uow.SaveAsync();

            return true;
        }
    }
}