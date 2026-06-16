using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IUniversityRepository
        : IGenericRepository<University, Guid>
    {
        Task<(IEnumerable<University>, int)> GetPagedAsync(
            string search,
            int page,
            int pageSize);
    }
}