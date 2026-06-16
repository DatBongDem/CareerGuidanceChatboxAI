using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUniversityService
    {
        Task<object> GetAll(string search, int page, int pageSize);
        Task<University?> GetById(Guid id);

        Task Create(University entity);
        Task Update(University entity);
        Task Delete(Guid id);
    }
}