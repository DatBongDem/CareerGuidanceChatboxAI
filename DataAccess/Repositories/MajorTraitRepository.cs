using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class MajorTraitRepository
        : GenericRepository<MajorTrait, Guid>, IMajorTraitRepository
    {
        private readonly ApplicationDbContext _context;

        public MajorTraitRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<MajorTrait>, int)> GetPagedAsync(
            Guid? majorId,
            Guid? traitId,
            int page,
            int pageSize)
        {
            var query = _context.Set<MajorTrait>()
                .Include(x => x.Major)
                .Include(x => x.Trait)
                .AsQueryable();

            if (majorId.HasValue)
                query = query.Where(x => x.MajorId == majorId);

            if (traitId.HasValue)
                query = query.Where(x => x.TraitId == traitId);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Weight)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
    }
}