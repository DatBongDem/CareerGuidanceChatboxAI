using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using System;

namespace DataAccess.Repositories
{
    public class ChatAiSummaryRepository : GenericRepository<ChatAiSummary, Guid>, IChatAiSummaryRepository
    {
        public ChatAiSummaryRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
