using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUniversityMajorService
    {
        Task<object> GetAll(string search, int page, int pageSize);
        Task<UniversityMajor?> GetById(Guid id);
        Task<object> GetByUniversity(Guid universityId, int page, int pageSize);
        Task<object> GetByMajor(Guid majorId, int page, int pageSize);

        Task Create(UniversityMajor entity);
        Task Update(UniversityMajor entity);
        Task Delete(Guid id);
        Task<object> Filter(
     Guid? universityId,
     Guid? majorId,
     int? year,
     double? minScore,
     double? maxScore,
     int page,
     int pageSize);
    }
}