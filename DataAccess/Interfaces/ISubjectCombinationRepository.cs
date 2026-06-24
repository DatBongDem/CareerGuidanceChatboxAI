using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface ISubjectCombinationRepository
        : IGenericRepository<SubjectCombination, Guid>
    {
        Task<(IEnumerable<SubjectCombination>, int)> GetPagedAsync(
            string search,
            int page,
            int pageSize);
    }
}