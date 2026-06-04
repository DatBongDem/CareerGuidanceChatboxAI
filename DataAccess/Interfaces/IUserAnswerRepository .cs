using DataAccess.Entities;

namespace DataAccess.Interfaces
{
    public interface IUserAnswerRepository
        : IGenericRepository<UserAnswer, Guid>
    {
    }
}
