using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class EduRegistrationRepository : GenericRepository<EduRegistration, Guid>, IEduRegistrationRepository
    {
        public EduRegistrationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<IEnumerable<EduRegistration>> GetAllAsync()
        {
            return await _dbSet
                .Include(er => er.Plan)
                .ToListAsync();
        }

        public override async Task<EduRegistration?> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(er => er.Plan)
                .FirstOrDefaultAsync(er => er.Id == id);
        }

        public async Task<EduRegistration?> GetByTransactionCodeAsync(string transactionCode)
        {
            return await _dbSet
                .Include(er => er.Plan)
                .FirstOrDefaultAsync(er => er.TransactionCode == transactionCode);
        }
    }
}
