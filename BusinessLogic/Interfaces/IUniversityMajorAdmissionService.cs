using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUniversityMajorAdmissionService
    {
        Task<object> GetAll(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            Guid? combinationId,
            int? year,
            double? minScore,
            double? maxScore,
            int page,
            int pageSize);

        Task<UniversityMajorAdmission?> GetById(Guid id);

        Task Create(UniversityMajorAdmission entity);
        Task Update(UniversityMajorAdmission entity);
        Task Delete(Guid id);
    }
}
