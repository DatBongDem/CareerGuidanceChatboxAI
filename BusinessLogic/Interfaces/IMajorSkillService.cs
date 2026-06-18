public interface IMajorSkillService
{
    Task<IEnumerable<MajorSkill>> GetAllAsync();
    Task<MajorSkill> CreateAsync(MajorSkill model);
}