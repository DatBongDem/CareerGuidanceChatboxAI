using DataAccess.Entities;
using System;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IEduActivationKeyRepository : IGenericRepository<EduActivationKey, Guid>
    {
        Task<EduActivationKey?> GetByKeyAsync(string key);
        Task<EduActivationKey?> GetByEmailAndRegistrationIdAsync(string email, Guid registrationId);
    }
}
