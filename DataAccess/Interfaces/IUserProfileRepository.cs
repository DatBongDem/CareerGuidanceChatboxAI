using DataAccess.Interfaces;

public interface IUserProfileRepository
    : IGenericRepository<UserProfile, Guid>
{
}
