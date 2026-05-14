using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Role;
using System; // Added for Guid
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<RoleDto>> GetAllRoles();
        Task<RoleDto> GetRoleById(Guid id);
        Task<RoleDto> CreateRole(CreateRoleDto createRoleDto);
        Task UpdateRole(Guid id, UpdateRoleDto updateRoleDto);
        Task DeleteRole(Guid id);
    }
}
