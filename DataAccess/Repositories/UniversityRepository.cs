using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class UniversityRepository : IUniversityRepository
    {
        private readonly ApplicationDbContext _context;

        public UniversityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<University>> GetAllAsync()
        {
            return await _context.Universities.ToListAsync();
        }

        public async Task<University?> GetByIdAsync(Guid id)
        {
            return await _context.Universities.FindAsync(id);
        }

        public async Task AddAsync(University university)
        {
            await _context.Universities.AddAsync(university);
        }

        public void Update(University university)
        {
            _context.Universities.Update(university);
        }

        public void Delete(University university)
        {
            _context.Universities.Remove(university);
        }
    }
}