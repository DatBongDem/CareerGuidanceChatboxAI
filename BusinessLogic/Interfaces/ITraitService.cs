using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface ITraitService
    {
        Task<object> GetAll(string search, int page, int pageSize);
        Task<Trait?> GetById(Guid id);

        Task Create(Trait entity);
        Task Update(Trait entity);
        Task Delete(Guid id);
    }
}
