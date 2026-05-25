using BusinessLogic.DTOs.User;
using System.Threading.Tasks;

namespace BusinessLogic.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> Login(LoginDto loginDto);
        Task<bool> RegisterStep1(RegisterStep1RequestDto request);
        Task<string> VerifyOtp(RegisterStep2RequestDto request);
        Task<bool> RegisterStep3(RegisterStep3RequestDto request);
        Task Logout(LogoutDto logoutDto);
        Task<MeResponseDto> GetMe(Guid userId);
        Task<LoginResponseDto> RefreshToken(string refreshToken);
    }
}
