using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class PlanHistoryRepository : GenericRepository<PlanHistory, Guid>, IPlanHistoryRepository
    {
        public PlanHistoryRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<PlanHistory>> GetByUserIdAsync(Guid userId)
        {
            return await _context.PlanHistories
                .Where(ph => ph.UserId == userId)
                .ToListAsync();
        }
    }
}
