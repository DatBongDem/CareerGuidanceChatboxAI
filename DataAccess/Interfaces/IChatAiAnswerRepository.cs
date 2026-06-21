using DataAccess.Entities.ChatAI;
using System;

namespace DataAccess.Interfaces
{
    public interface IChatAiAnswerRepository : IGenericRepository<ChatAiAnswer, Guid>
    {
    }
}
