using DataAccess.DataContext;
using DataAccess.Repositories;

public class MajorRepository
    : GenericRepository<Major, Guid>, IMajorRepository
{
    public MajorRepository(ApplicationDbContext context)
        : base(context)
    {
    }
}
