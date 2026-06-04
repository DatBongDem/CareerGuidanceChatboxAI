public interface IUserProfileService
{
    Task<IEnumerable<UserProfile>> GetAllAsync();
    Task<UserProfile?> GetByIdAsync(Guid id);
    Task<UserProfile> CreateAsync(UserProfile model);
    Task<bool> UpdateAsync(Guid id, UserProfile model);
    Task<bool> DeleteAsync(Guid id);
}