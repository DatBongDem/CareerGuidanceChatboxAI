using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IUniversityMajorAdmissionRepository
        : IGenericRepository<UniversityMajorAdmission, Guid>
    {
        Task<(IEnumerable<UniversityMajorAdmission>, int)> GetPagedAsync(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            Guid? combinationId,
            int? year,
            double? minScore,
            double? maxScore,
            int page,
            int pageSize);
    }
}