using DataAccess.Entities;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IUserRepository : IGenericRepository<User, Guid>
    {
        Task<User?> GetByEmailAsync(string email);
    }
}
