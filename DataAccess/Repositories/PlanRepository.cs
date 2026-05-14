using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class PlanRepository : GenericRepository<Plan, Guid>, IPlanRepository
    {
        public PlanRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Plan?> GetPlanByNameAsync(string planName)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.Name == planName);
        }
    }
}
