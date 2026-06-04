using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public class UserAnswerRepository
        : GenericRepository<UserAnswer, Guid>, IUserAnswerRepository
    {
        public UserAnswerRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}