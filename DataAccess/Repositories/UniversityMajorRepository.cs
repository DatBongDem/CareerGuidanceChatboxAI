using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class UniversityMajorRepository
        : GenericRepository<UniversityMajor, Guid>, IUniversityMajorRepository
    {
        private readonly ApplicationDbContext _context;

        public UniversityMajorRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<UniversityMajor>, int)> GetPagedAsync(
         string search,
         int page,
         int pageSize)
        {
            var query = _context.UniversityMajors
                .Include(x => x.University)
                .Include(x => x.Major)
                .AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower(); 

                query = query.Where(x =>
                    (x.University.Name != null && x.University.Name.ToLower().Trim().Contains(search)) ||
                    (x.University.ShortName != null && x.University.ShortName.ToLower().Trim().Contains(search)) ||
                    (x.Major.Name != null && x.Major.Name.ToLower().Trim().Contains(search))
                );
            }


            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<(IEnumerable<UniversityMajor>, int)> GetByUniversityAsync(
            Guid universityId,
            int page,
            int pageSize)
        {
            var query = _context.Set<UniversityMajor>()
                .Include(x => x.Major)
                .Where(x => x.UniversityId == universityId);

            var total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }

        public async Task<(IEnumerable<UniversityMajor>, int)> GetByMajorAsync(
            Guid majorId,
            int page,
            int pageSize)
        {
            var query = _context.Set<UniversityMajor>()
                .Include(x => x.University)
                .Where(x => x.MajorId == majorId);

            var total = await query.CountAsync();

            var data = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
        public async Task<(IEnumerable<UniversityMajor>, int)> FilterAsync(
    Guid? universityId,
    Guid? majorId,
    int? year,
    double? minScore,
    double? maxScore,
    int page,
    int pageSize)
        {
            var query = _context.Set<UniversityMajor>()
                .Include(x => x.University)
                .Include(x => x.Major)
                .AsQueryable();

           
            if (universityId.HasValue)
            {
                query = query.Where(x => x.UniversityId == universityId.Value);
            }

           
            if (majorId.HasValue)
            {
                query = query.Where(x => x.MajorId == majorId.Value);
            }

            // ✅ lọc theo năm
            if (year.HasValue)
            {
                query = query.Where(x => x.Year == year.Value);
            }

            // ✅ lọc theo điểm
            if (minScore.HasValue)
            {
                query = query.Where(x => x.CutoffScore >= minScore.Value);
            }

            if (maxScore.HasValue)
            {
                query = query.Where(x => x.CutoffScore <= maxScore.Value);
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.CutoffScore)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
    }
}