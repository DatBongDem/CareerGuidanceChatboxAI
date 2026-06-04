using DataAccess.DataContext;
using DataAccess.Repositories;

public class UserProfileRepository
    : GenericRepository<UserProfile, Guid>, IUserProfileRepository
{
    public UserProfileRepository(ApplicationDbContext context)
        : base(context)
    {
    }
}