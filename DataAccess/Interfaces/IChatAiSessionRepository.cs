using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IChatAiSessionRepository : IGenericRepository<ChatAiSession, Guid>
    {
    }
}
