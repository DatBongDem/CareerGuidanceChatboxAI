using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class EduActivationKeyRepository : GenericRepository<EduActivationKey, Guid>, IEduActivationKeyRepository
    {
        public EduActivationKeyRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<EduActivationKey?> GetByKeyAsync(string key)
        {
            return await _dbSet
                .Include(eak => eak.Registration)
                    .ThenInclude(r => r!.Plan)
                .Include(eak => eak.UsedByUser)
                .FirstOrDefaultAsync(eak => eak.ActivationKey == key);
        }

        public async Task<EduActivationKey?> GetByEmailAndRegistrationIdAsync(string email, Guid registrationId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(eak => eak.Email == email && eak.RegistrationId == registrationId);
        }
    }
}
