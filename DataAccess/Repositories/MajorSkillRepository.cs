using DataAccess.DataContext;
using DataAccess.Repositories;

public class MajorSkillRepository
    : GenericRepository<MajorSkill, Guid>, IMajorSkillRepository
{
    public MajorSkillRepository(ApplicationDbContext context)
        : base(context)
    {
    }
}