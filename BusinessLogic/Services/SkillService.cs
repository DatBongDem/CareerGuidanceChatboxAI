using DataAccess.Interfaces;

public class SkillService : ISkillService
{
    private readonly IUnitOfWork _uow;

    public SkillService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Skill>> GetAllAsync()
    {
        return await _uow.SkillRepository.GetAllAsync();
    }

    public async Task<Skill?> GetByIdAsync(Guid id)
    {
        return await _uow.SkillRepository.GetByIdAsync(id);
    }

    public async Task<Skill> CreateAsync(Skill model)
    {
        model.SkillId = Guid.NewGuid();

        await _uow.SkillRepository.AddAsync(model);
        await _uow.SaveAsync();

        return model;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _uow.SkillRepository.GetByIdAsync(id);
        if (existing == null) return false;

        await _uow.SkillRepository.DeleteAsync(id);
        await _uow.SaveAsync();

        return true;
    }
}