using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class CampusRepository
        : GenericRepository<Campus, Guid>, ICampusRepository
    {
        private readonly ApplicationDbContext _context;

        public CampusRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Campus>, int)> GetPagedAsync(
            Guid? universityId,
            string search,
            int page,
            int pageSize)
        {
            var query = _context.Set<Campus>()
                .Include(x => x.University)
                .AsQueryable();

            if (universityId.HasValue)
                query = query.Where(x => x.UniversityId == universityId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(search)) ||
                    (x.Address != null && x.Address.ToLower().Contains(search)) ||
                    (x.University.Name != null && x.University.Name.ToLower().Contains(search))
                );
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
    }
}