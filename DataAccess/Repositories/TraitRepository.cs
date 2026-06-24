using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class TraitRepository
        : GenericRepository<Trait, Guid>, ITraitRepository
    {
        private readonly ApplicationDbContext _context;

        public TraitRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<Trait>, int)> GetPagedAsync(
            string search,
            int page,
            int pageSize)
        {
            var query = _context.Set<Trait>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(x =>
                    x.Name != null &&
                    x.Name.ToLower().Contains(search)
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