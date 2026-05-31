using DataAccess.DataContext;
using DataAccess.Entities.ChatAI;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class ChatHistoryRepository
    : GenericRepository<ChatHistory, Guid>,
      IChatHistoryRepository
    {
        public ChatHistoryRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<ChatHistory>>
            GetByUserIdAsync(Guid userId)
        {
            return await _dbSet.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedAt).ToListAsync();
        }
    }
}
