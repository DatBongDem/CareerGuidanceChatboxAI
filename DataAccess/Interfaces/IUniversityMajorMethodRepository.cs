using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IUniversityMajorMethodRepository
        : IGenericRepository<UniversityMajorMethod, Guid>
    {
        Task<(IEnumerable<UniversityMajorMethod>, int)> GetPagedAsync(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            int page,
            int pageSize);
    }
}