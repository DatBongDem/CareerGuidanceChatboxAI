using BusinessLogic.DTOs.User;
using DataAccess.Entities;

namespace BusinessLogic.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(Guid id);
        Task<UserDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<bool> UpdateUserAsync(Guid id, UpdateUserDto updateUserDto);
        Task<User?> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto);
        Task<bool> DeleteUserAsync(Guid id);
        Task<bool> ToggleUserActiveStatusAsync(Guid id);
    }
}
