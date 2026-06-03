using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace DataAccess.Repositories
{
    public class UniversityRepository
        : GenericRepository<University, Guid>, IUniversityRepository
    {
        public UniversityRepository(ApplicationDbContext context)
            : base(context)
        {
        }
    }
}
