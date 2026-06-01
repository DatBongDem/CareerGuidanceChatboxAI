using DataAccess.Entities.ChatAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IChatHistoryRepository
    : IGenericRepository<ChatHistory, Guid>
    {
        Task<IEnumerable<ChatHistory>>
            GetByUserIdAsync(Guid userId);
    }
}
