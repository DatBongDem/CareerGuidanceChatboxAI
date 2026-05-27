using DataAccess.DataContext;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repositories
{
    public class EmailVerificationRepository
        : GenericRepository<EmailVerification, Guid>,
          IEmailVerificationRepository
    {
        public EmailVerificationRepository(
            ApplicationDbContext context)
            : base(context)
        {
        }

        public async Task<EmailVerification?>
            GetByEmailAndOtpAsync(
                string email,
                string otp)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ev =>
                    ev.Email == email &&
                    ev.Otp == otp &&
                    !ev.IsUsed);
        }

        public async Task<EmailVerification?>
            GetByVerifyTokenAsync(
                string verifyToken)
        {
            return await _dbSet
                .FirstOrDefaultAsync(ev =>
                    ev.VerifyToken == verifyToken &&
                    !ev.IsUsed);
        }

        public async Task<List<EmailVerification>>
            GetByEmailAsync(string email)
        {
            return await _dbSet
                .Where(ev => ev.Email == email)
                .ToListAsync();
        }
    }
}