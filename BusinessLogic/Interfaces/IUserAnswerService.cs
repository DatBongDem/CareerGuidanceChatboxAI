using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUserAnswerService
    {
        Task<IEnumerable<UserAnswer>> GetAllAsync();
        Task<UserAnswer?> GetByIdAsync(Guid id);
        Task<UserAnswer> CreateAsync(UserAnswer model);
        Task<bool> DeleteAsync(Guid id);
    }
}
