using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class SubjectCombinationRepository
        : GenericRepository<SubjectCombination, Guid>, ISubjectCombinationRepository
    {
        private readonly ApplicationDbContext _context;

        public SubjectCombinationRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<(IEnumerable<SubjectCombination>, int)> GetPagedAsync(
            string search,
            int page,
            int pageSize)
        {
            var query = _context.Set<SubjectCombination>().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.Code != null && x.Code.ToLower().Contains(search)) ||
                    (x.Subjects != null && x.Subjects.ToLower().Contains(search))
                );
            }

            var total = await query.CountAsync();

            var data = await query
                .OrderBy(x => x.Code)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (data, total);
        }
    }
}
