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
            var question = await _uow.QuestionRepository.GetByIdAsync(model.QuestionId);
            if (question == null)
            {
                throw new Exception("Không tìm thấy câu hỏi");
            }

            // Check if this user has already answered this question
            var existing = await _uow.UserAnswerRepository.GetAsync(a => a.UserId == model.UserId && a.QuestionId == model.QuestionId);
            if (existing.Any())
            {
                throw new Exception("Câu hỏi này bạn đã trả lời rồi");
            }

            model.UserAnswerId = Guid.NewGuid();
            model.AnsweredAt = DateTime.UtcNow;

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

        public async Task<IEnumerable<UserAnswer>> GetByUserIdAsync(Guid userId)
        {
            return await _uow.UserAnswerRepository.GetAsync(a => a.UserId == userId);
        }

        public async Task<bool> DeleteByUserIdAsync(Guid userId)
        {
            var answers = await _uow.UserAnswerRepository.GetAsync(a => a.UserId == userId);
            var answersList = answers.ToList();
            if (!answersList.Any()) return false;

            foreach (var answer in answersList)
            {
                await _uow.UserAnswerRepository.DeleteAsync(answer.UserAnswerId);
            }
            await _uow.SaveAsync();

            return true;
        }
    }
}