using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IUniversityMajorRepository
        : IGenericRepository<UniversityMajor, Guid>
    {
        Task<(IEnumerable<UniversityMajor>, int)> GetPagedAsync(
            string search,
            int page,
            int pageSize);

        Task<(IEnumerable<UniversityMajor>, int)> GetByUniversityAsync(
            Guid universityId,
            int page,
            int pageSize);

        Task<(IEnumerable<UniversityMajor>, int)> GetByMajorAsync(
            Guid majorId,
            int page,
            int pageSize);
        Task<(IEnumerable<UniversityMajor>, int)> FilterAsync(
    Guid? universityId,
    Guid? majorId,
    int? year,
    double? minScore,
    double? maxScore,
    int page,
    int pageSize);

    }
}