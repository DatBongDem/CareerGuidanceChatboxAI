public interface IMajorSkillService
{
    Task<IEnumerable<MajorSkill>> GetAllAsync();
    Task<MajorSkill?> GetByIdAsync(Guid id);
    Task<MajorSkill> CreateAsync(MajorSkill model);
    Task<bool> UpdateAsync(Guid id, MajorSkill model);
    Task<bool> DeleteAsync(Guid id);
}