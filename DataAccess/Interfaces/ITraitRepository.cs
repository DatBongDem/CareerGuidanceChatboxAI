using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface ITraitRepository
        : IGenericRepository<Trait, Guid>
    {
        Task<(IEnumerable<Trait>, int)> GetPagedAsync(
            string search,
            int page,
            int pageSize);
    }
}
