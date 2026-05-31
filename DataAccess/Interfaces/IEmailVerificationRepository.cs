using DataAccess.Entities;
using System;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IEmailVerificationRepository : IGenericRepository<EmailVerification, Guid>
    {
        Task<EmailVerification?> GetByEmailAndOtpAsync(string email, string otp);
        Task<EmailVerification?> GetByVerifyTokenAsync(string verifyToken);
        Task<List<EmailVerification>> GetByEmailAsync(string email);
    }
}
