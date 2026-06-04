using DataAccess.Interfaces;

public class MajorService : IMajorService
{
    private readonly IUnitOfWork _uow;

    public MajorService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<IEnumerable<Major>> GetAllAsync()
    {
        return await _uow.MajorRepository.GetAllAsync();
    }

    public async Task<Major?> GetByIdAsync(Guid id)
    {
        return await _uow.MajorRepository.GetByIdAsync(id);
    }

    public async Task<Major> CreateAsync(Major model)
    {
        model.MajorId = Guid.NewGuid();

        await _uow.MajorRepository.AddAsync(model);
        await _uow.SaveAsync();

        return model;
    }
    public async Task<bool> UpdateAsync(Guid id, Major model)
    {
        var existing = await _uow.MajorRepository.GetByIdAsync(id);
        if (existing == null) return false;

        existing.Name = model.Name;
        existing.Description = model.Description;

        await _uow.MajorRepository.UpdateAsync(existing);
        await _uow.SaveAsync();

        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var existing = await _uow.MajorRepository.GetByIdAsync(id);
        if (existing == null) return false;

        await _uow.MajorRepository.DeleteAsync(id);
        await _uow.SaveAsync();

        return true;
    }
}