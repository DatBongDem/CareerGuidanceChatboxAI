using DataAccess.Interfaces;

public class MajorSkillService : IMajorSkillService
{
    private readonly IUnitOfWork _uow;

    public MajorSkillService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<MajorSkill>> GetAllAsync()
    {
        return await _uow.MajorSkillRepository.GetAllAsync();
    }

    public async Task<MajorSkill?> GetByIdAsync(Guid id)
    {
        return await _uow.MajorSkillRepository.GetByIdAsync(id);
    }

    public async Task<MajorSkill> CreateAsync(MajorSkill model)
    {
        model.Id = Guid.NewGuid();

        await _uow.MajorSkillRepository.AddAsync(model);
        await _uow.SaveAsync();

        return model;
    }

    public async Task<bool> UpdateAsync(Guid id, MajorSkill model)
    {
        var existingMajorSkill = await _uow.MajorSkillRepository.GetByIdAsync(id);

        if (existingMajorSkill == null)
            return false;

        existingMajorSkill.MajorId = model.MajorId;
        existingMajorSkill.SkillId = model.SkillId;
        existingMajorSkill.Weight = model.Weight;

        await _uow.MajorSkillRepository.UpdateAsync(existingMajorSkill);
        await _uow.SaveAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existingMajorSkill = await _uow.MajorSkillRepository.GetByIdAsync(id);

        if (existingMajorSkill == null)
            return false;

        await _uow.MajorSkillRepository.DeleteAsync(id);
        await _uow.SaveAsync();

        return true;
    }
}