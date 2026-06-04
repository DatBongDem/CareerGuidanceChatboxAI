using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore; // Needed for FirstOrDefaultAsync

namespace DataAccess.Repositories
{
    public class UserRepository : GenericRepository<User, Guid>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _dbSet
                .Include(u => u.Role)
                .Include(u => u.PlanHistories)
                    .ThenInclude(ph => ph.Plan)
                .ToListAsync();
        }

        public override async Task<User?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.Role)
                .Include(u => u.PlanHistories)
                    .ThenInclude(ph => ph.Plan)
                .FirstOrDefaultAsync(u => u.UserId == id);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
