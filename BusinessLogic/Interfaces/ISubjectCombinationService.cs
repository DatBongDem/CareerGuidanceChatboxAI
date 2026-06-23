using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface ISubjectCombinationService
    {
        Task<object> GetAll(string search, int page, int pageSize);
        Task<SubjectCombination?> GetById(Guid id);

        Task Create(SubjectCombination entity);
        Task Update(SubjectCombination entity);
        Task Delete(Guid id);
    }
}
