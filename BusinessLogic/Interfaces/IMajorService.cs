public interface IMajorService
{
    Task<IEnumerable<Major>> GetAllAsync();
    Task<Major?> GetByIdAsync(Guid id);
    Task<Major> CreateAsync(Major model);

    Task<bool> UpdateAsync(Guid id, Major model); 

    Task<bool> DeleteAsync(Guid id);
}