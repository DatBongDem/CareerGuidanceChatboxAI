using DataAccess.Entities;
using System;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IEduRegistrationRepository : IGenericRepository<EduRegistration, Guid>
    {
        Task<EduRegistration?> GetByTransactionCodeAsync(string transactionCode);
    }
}
