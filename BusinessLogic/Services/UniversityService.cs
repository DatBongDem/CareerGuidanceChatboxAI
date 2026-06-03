using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class UniversityService : IUniversityService
    {
        private readonly IUnitOfWork _uow;

        public UniversityService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<University>> GetAllAsync()
        {
            return await _uow.Universities.GetAllAsync();
        }

        public async Task<University?> GetByIdAsync(Guid id)
        {
            return await _uow.Universities.GetByIdAsync(id);
        }

        public async Task<University> CreateAsync(University model)
        {
            model.UniversityId = Guid.NewGuid();

            await _uow.Universities.AddAsync(model);
            await _uow.SaveAsync();

            return model;
        }

        public async Task<bool> UpdateAsync(Guid id, University model)
        {
            var existing = await _uow.Universities.GetByIdAsync(id);
            if (existing == null) return false;

            existing.Name = model.Name;
            existing.ShortName = model.ShortName;
            existing.Location = model.Location;
            existing.Ranking = model.Ranking;
            existing.Avatar = model.Avatar;

            await _uow.Universities.UpdateAsync(existing);
            await _uow.SaveAsync();

            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            await _uow.Universities.DeleteAsync(id);
            await _uow.SaveAsync();

            return true;
        }
    }
}
