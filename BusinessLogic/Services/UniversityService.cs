using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;

namespace BusinessLogic.Services
{
    public class UniversityService : IUniversityService
    {
        private readonly IUniversityRepository _repo;

        public UniversityService(IUniversityRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<University>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<University?> GetByIdAsync(Guid id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<University> CreateAsync(University model)
        {
            model.UniversityId = Guid.NewGuid();

            await _repo.AddAsync(model);
            await Save();

            return model;
        }

        public async Task<University?> UpdateAsync(Guid id, University model)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return null;

            existing.Name = model.Name;
            existing.ShortName = model.ShortName;
            existing.Location = model.Location;
            existing.Ranking = model.Ranking;
            existing.Avatar = model.Avatar;

            _repo.Update(existing);
            await Save();

            return existing;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var existing = await _repo.GetByIdAsync(id);
            if (existing == null) return false;

            _repo.Delete(existing);
            await Save();

            return true;
        }

        // nếu mày có UnitOfWork thì thay phần này
        private async Task Save()
        {
            // quick fix nếu chưa dùng UoW
            // inject DbContext vào repo thì phải expose SaveChanges
            // hoặc dùng UnitOfWork đúng chuẩn
        }
    }
}
