using AutoMapper;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.Role;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System; // Added for Guid
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class RoleService : IRoleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RoleService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRoles()
        {
            var roles = await _unitOfWork.RoleRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> GetRoleById(Guid id)
        {
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(id);
            return _mapper.Map<RoleDto>(role);
        }

        public async Task<RoleDto> CreateRole(CreateRoleDto createRoleDto)
        {
            var role = _mapper.Map<Role>(createRoleDto);
            await _unitOfWork.RoleRepository.AddAsync(role);
            await _unitOfWork.SaveAsync();
            return _mapper.Map<RoleDto>(role);
        }

        public async Task UpdateRole(Guid id, UpdateRoleDto updateRoleDto)
        {
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return;
            }

            _mapper.Map(updateRoleDto, role);
            await _unitOfWork.RoleRepository.UpdateAsync(role);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteRole(Guid id)
        {
            var role = await _unitOfWork.RoleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return;
            }
            await _unitOfWork.RoleRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
        }
    }
}
