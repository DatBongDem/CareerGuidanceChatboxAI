using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUniversityMajorMethodService
    {
        Task<object> GetAll(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            int page,
            int pageSize);

        Task<UniversityMajorMethod?> GetById(Guid id);

        Task Create(UniversityMajorMethod entity);
        Task Update(UniversityMajorMethod entity);
        Task Delete(Guid id);
    }
}