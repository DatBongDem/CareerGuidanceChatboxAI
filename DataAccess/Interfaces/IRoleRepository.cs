using DataAccess.Entities;
using System; // Added for Guid
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role, Guid>
    {
        Task<Role?> GetRoleByNameAsync(string roleName);
    }
}
