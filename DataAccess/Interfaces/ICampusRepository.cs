using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface ICampusRepository
        : IGenericRepository<Campus, Guid>
    {
        Task<(IEnumerable<Campus>, int)> GetPagedAsync(
            Guid? universityId,
            string search,
            int page,
            int pageSize);
    }
}