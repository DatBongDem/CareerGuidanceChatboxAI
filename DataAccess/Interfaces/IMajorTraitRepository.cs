using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IMajorTraitRepository
        : IGenericRepository<MajorTrait, Guid>
    {
        Task<(IEnumerable<MajorTrait>, int)> GetPagedAsync(
            Guid? majorId,
            Guid? traitId,
            int page,
            int pageSize);
    }
}
