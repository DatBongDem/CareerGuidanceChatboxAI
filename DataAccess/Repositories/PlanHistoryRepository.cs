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
    public class PlanHistoryRepository
        : GenericRepository<PlanHistory, Guid>,
          IPlanHistoryRepository
    {
        public PlanHistoryRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<PlanHistory>>
            GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Plan)
                .Include(x => x.Transaction)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync();
        }

        public async Task<PlanHistory?>
            GetLatestByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(x => x.Plan)
                .Include(x => x.Transaction)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.StartDate)
                .FirstOrDefaultAsync();
        }

        public override async Task<PlanHistory?>
            GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Plan)
                .Include(x => x.Transaction)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
