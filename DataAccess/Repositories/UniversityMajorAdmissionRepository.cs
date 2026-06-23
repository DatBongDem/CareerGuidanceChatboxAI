using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class UniversityMajorAdmissionRepository
        : GenericRepository<UniversityMajorAdmission, Guid>, IUniversityMajorAdmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public UniversityMajorAdmissionRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<UniversityMajorAdmission>, int)> GetPagedAsync(
            Guid? universityId,
            Guid? majorId,
            Guid? methodId,
            Guid? combinationId,
            int? year,
            double? minScore,
            double? maxScore,
            int page,
            int pageSize)
        {
            var query = _context.Set<UniversityMajorAdmission>()
                .Include(x => x.University)
                .Include(x => x.Major)
                .Include(x => x.Campus)
                .Include(x => x.AdmissionMethod)
                .Include(x => x.SubjectCombination)
                .AsQueryable();

            if (universityId.HasValue)
                query = query.Where(x => x.UniversityId == universityId);

            if (majorId.HasValue)
                query = query.Where(x => x.MajorId == majorId);

            if (methodId.HasValue)
                query = query.Where(x => x.MethodId == methodId);

            if (combinationId.HasValue)
                query = query.Where(x => x.CombinationId == combinationId);

            if (year.HasValue)
                query = query.Where(x => x.Year == year);

            if (minScore.HasValue)
                query = query.Where(x => x.CutoffScore >= minScore);

            if (maxScore.HasValue)
                query = query.Where(x => x.CutoffScore <= maxScore);

            var total = await query.CountAsync();

            var data = await query
                .OrderByDescending(x => x.Year)
                .ThenByDescending(x => x.CutoffScore)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            return (data, total);
        }
    }
}