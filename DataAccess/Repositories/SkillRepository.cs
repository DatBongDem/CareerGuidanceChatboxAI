using DataAccess.DataContext;
using DataAccess.Repositories;

public class SkillRepository
    : GenericRepository<Skill, Guid>, ISkillRepository
{
    public SkillRepository(ApplicationDbContext context)
        : base(context)
    {
    }
}
