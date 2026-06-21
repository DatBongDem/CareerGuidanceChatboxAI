using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class ChatAiAnswerRepository : GenericRepository<ChatAiAnswer, Guid>, IChatAiAnswerRepository
    {
        public ChatAiAnswerRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
