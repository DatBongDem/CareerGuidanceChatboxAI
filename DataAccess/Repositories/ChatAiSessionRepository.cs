using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class ChatAiSessionRepository : GenericRepository<ChatAiSession, Guid>, IChatAiSessionRepository
    {
        public ChatAiSessionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
