using AutoMapper;
using BusinessLogic.DTOs.User;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using System; // Added for Guid
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessLogic.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UserService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            
            // Basic password hashing, consider using a more robust library like BCrypt.Net
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
            user.UserId = Guid.NewGuid(); // Assign a new Guid for UserId
            user.CreateAt = DateTime.UtcNow; // Set creation timestamp
            user.UpdateAt = DateTime.UtcNow; // Set update timestamp

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();

            // We need to fetch the created user to get related data (Role, Plan)
            // Use UserId for GetByIdAsync
            var createdUser = await _unitOfWork.UserRepository.GetByIdAsync(user.UserId);
            return _mapper.Map<UserDto>(createdUser);
        }

        public async Task<bool> DeleteUserAsync(Guid id) // Changed int to Guid
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _unitOfWork.UserRepository.DeleteAsync(id);
            await _unitOfWork.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.UserRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> GetUserByIdAsync(Guid id) // Changed int to Guid
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            return _mapper.Map<UserDto>(user);
        }

        public async Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto) // Changed int to Guid
        {
            var user = await _unitOfWork.UserRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            _mapper.Map(updateUserDto, user);

            // Handle password update if provided
            if (!string.IsNullOrEmpty(updateUserDto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserDto.Password);
            }
            user.UpdateAt = DateTime.UtcNow; // Update timestamp

            await _unitOfWork.UserRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
