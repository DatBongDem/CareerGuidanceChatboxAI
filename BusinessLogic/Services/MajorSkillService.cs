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

    public async Task<MajorSkill> CreateAsync(MajorSkill model)
    {
        model.Id = Guid.NewGuid();

        await _uow.MajorSkillRepository.AddAsync(model);
        await _uow.SaveAsync();

        return model;
    }
}