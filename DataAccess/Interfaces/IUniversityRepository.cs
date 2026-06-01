using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IUniversityRepository
    {
        Task<List<University>> GetAllAsync();
        Task<University?> GetByIdAsync(Guid id);
        Task AddAsync(University university);
        void Update(University university);
        void Delete(University university);
    }
}
