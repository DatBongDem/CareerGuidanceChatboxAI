using BusinessLogic.DTOs.User;
using BusinessLogic.DTOs.User.ForgetPassword;
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
        Task<MeResponseDto> UpdateProfileAsync(Guid userId, UpdateProfileDto updateProfileDto);
        Task ForgotPassword(ForgotPasswordDto dto);
        Task ResetPassword(ResetPasswordDto dto);
        Task ChangePassword(string email, ChangePasswordDto dto);
    }
}
