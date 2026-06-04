using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUniversityService
    {
        Task<IEnumerable<University>> GetAllAsync();
        Task<University?> GetByIdAsync(Guid id);
        Task<University> CreateAsync(University model);
        Task<bool> UpdateAsync(Guid id, University model);
        Task<bool> DeleteAsync(Guid id);
    }
}
