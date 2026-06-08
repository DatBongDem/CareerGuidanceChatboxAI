public interface ISkillService
{
    Task<IEnumerable<Skill>> GetAllAsync();
    Task<Skill?> GetByIdAsync(Guid id);
    Task<Skill> CreateAsync(Skill model);
    Task<bool> DeleteAsync(Guid id);
}
