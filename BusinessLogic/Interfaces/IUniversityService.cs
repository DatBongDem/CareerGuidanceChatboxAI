using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUniversityService
    {
        Task<List<University>> GetAllAsync();
        Task<University?> GetByIdAsync(Guid id);
        Task<University> CreateAsync(University model);
        Task<University?> UpdateAsync(Guid id, University model);
        Task<bool> DeleteAsync(Guid id);
    }
}