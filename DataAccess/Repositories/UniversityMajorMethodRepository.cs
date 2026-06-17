using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class UniversityMajorMethodRepository
        : GenericRepository<UniversityMajorMethod, Guid>, IUniversityMajorMethodRepository
    {
        private readonly ApplicationDbContext _context;

        public UniversityMajorMethodRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<UniversityMajorMethod>, int)> GetPagedAsync(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            int page,
            int pageSize)
        {
            var query = _context.Set<UniversityMajorMethod>()
                .Include(x => x.University)
                .Include(x => x.Major)
                .Include(x => x.AdmissionMethod) // ✅ FIX LỖI
                .AsQueryable();

            if (universityId.HasValue)
                query = query.Where(x => x.UniversityId == universityId.Value);

            if (majorId.HasValue)
                query = query.Where(x => x.MajorId == majorId.Value);

            if (methodId.HasValue)
                query = query.Where(x => x.MethodId == methodId.Value);

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.UniversityId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
    }
}
