using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IUserAiSummaryRepository : IGenericRepository<UserAiSummary, Guid>
    {
    }
}
