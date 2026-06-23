using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IMajorTraitService
    {
        Task<object> GetAll(Guid? majorId, Guid? traitId, int page, int pageSize);
        Task<MajorTrait?> GetById(Guid id);

        Task Create(MajorTrait entity);
        Task Update(MajorTrait entity);
        Task Delete(Guid id);
    }
}
