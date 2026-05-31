using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class PaymentTransactionRepository
        : GenericRepository<PaymentTransaction, Guid>,
          IPaymentTransactionRepository
    {
        public PaymentTransactionRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<PaymentTransaction?>
            GetByTransactionCodeAsync(string code)
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x =>
                    x.TransactionCode == code);
        }

        public async Task<IEnumerable<PaymentTransaction>>
            GetByUserIdAsync(Guid userId)
        {
            return await _dbSet
                .Include(x => x.Plan)
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public override async Task<PaymentTransaction?>
            GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(x => x.User)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(x =>
                    x.TransactionId == id);
        }
    }
}
