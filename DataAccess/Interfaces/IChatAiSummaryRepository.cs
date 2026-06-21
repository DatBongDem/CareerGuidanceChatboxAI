using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IChatAiSummaryRepository : IGenericRepository<ChatAiSummary, Guid>
    {
    }
}
