using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class UserAiSummaryRepository : GenericRepository<UserAiSummary, Guid>, IUserAiSummaryRepository
    {
        public UserAiSummaryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
