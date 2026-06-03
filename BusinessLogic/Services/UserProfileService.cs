using DataAccess.Interfaces;

public class UserProfileService : IUserProfileService
{
    private readonly IUnitOfWork _uow;

    public UserProfileService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<UserProfile>> GetAllAsync()
    {
        return await _uow.UserProfileRepository.GetAllAsync();
    }

    public async Task<UserProfile?> GetByIdAsync(Guid id)
    {
        return await _uow.UserProfileRepository.GetByIdAsync(id);
    }

    public async Task<UserProfile> CreateAsync(UserProfile model)
    {
        model.ProfileId = Guid.NewGuid();

        await _uow.UserProfileRepository.AddAsync(model);
        await _uow.SaveAsync();

        return model;
    }

    public async Task<bool> UpdateAsync(Guid id, UserProfile model)
    {
        var existing = await _uow.UserProfileRepository.GetByIdAsync(id);
        if (existing == null) return false;

        existing.GPA = model.GPA;
        existing.StrengthSubjects = model.StrengthSubjects;
        existing.Interests = model.Interests;
        existing.Personality = model.Personality;
        existing.CareerGoals = model.CareerGoals;

        await _uow.UserProfileRepository.UpdateAsync(existing);
        await _uow.SaveAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _uow.UserProfileRepository.GetByIdAsync(id);
        if (existing == null) return false;

        await _uow.UserProfileRepository.DeleteAsync(id);
        await _uow.SaveAsync();

        return true;
    }
}