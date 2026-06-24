using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface ICampusService
    {
        Task<object> GetAll(Guid? universityId, string search, int page, int pageSize);
        Task<Campus?> GetById(Guid id);

        Task Create(Campus entity);
        Task Update(Campus entity);
        Task Delete(Guid id);
    }
}
