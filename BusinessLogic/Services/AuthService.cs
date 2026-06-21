using AutoMapper;
using BCrypt.Net;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.User;
using BusinessLogic.DTOs.User.ForgetPassword;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace BusinessLogic.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IPlanService _planService;
        private readonly IEmailService _emailService;
        private readonly IEmailTemplateService _templateService;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;

        public AuthService(
            IUnitOfWork unitOfWork,
            IUserService userService,
            IRoleService roleService,
            IPlanService planService,
            IEmailService emailService,
            IEmailTemplateService templateService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _roleService = roleService;
            _planService = planService;
            _emailService = emailService;
            _templateService = templateService;
            _configuration = configuration;
            _mapper = mapper;
        }

        // ========================= LOGIN =========================

        public async Task<LoginResponseDto> Login(LoginDto loginDto)
        {
            var user = await _unitOfWork.UserRepository
                .GetByEmailAsync(loginDto.Email);

            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(
                    loginDto.Password,
                    user.PasswordHash))
            {
                throw new ApplicationException(
                    "Invalid email or password.");
            }

            if (!user.IsActive)
            {
                throw new ApplicationException(
                    "Account is inactive.");
            }

            var role = await _unitOfWork.RoleRepository
                .GetByIdAsync(user.RoleId);

            var roleName = role?.Name ?? "User";

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.Role,
                    roleName),

                new Claim(
                    "UserId",
                    user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var accessToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],

                audience: _configuration["Jwt:Audience"],

                claims: claims,

                expires: DateTime.UtcNow.AddMinutes(15),

                signingCredentials: creds
            );

            var accessTokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(accessToken);

            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),

                UserId = user.UserId,

                TokenHash = BCrypt.Net.BCrypt
                    .HashPassword(refreshToken),

                CreatedAt = DateTime.UtcNow,

                ExpiresAt = DateTime.UtcNow.AddDays(7),

                DeviceInfo = "Unknown",

                IpAddress = "Unknown"
            };

            await _unitOfWork.RefreshTokenRepository
                .AddAsync(refreshTokenEntity);

            user.LastLoginTime = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();

            return new LoginResponseDto
            {
                AccessToken = accessTokenString,

                RefreshToken = refreshToken
            };
        }

        // ========================= LOGOUT =========================

        public async Task Logout(LogoutDto logoutDto)
        {
            var refreshTokenEntity = await _unitOfWork
                .RefreshTokenRepository
                .GetByTokenAsync(logoutDto.RefreshToken);

            if (refreshTokenEntity == null)
            {
                throw new ApplicationException(
                    "Invalid refresh token.");
            }

            refreshTokenEntity.RevokedAt =
                DateTime.UtcNow;

            await _unitOfWork
                .RefreshTokenRepository
                .UpdateAsync(refreshTokenEntity);

            await _unitOfWork.SaveAsync();
        }

        // ========================= REGISTER STEP 1 =========================

        public async Task<bool> RegisterStep1(
            RegisterStep1RequestDto request)
        {
            var existingUser = await _unitOfWork
                .UserRepository
                .GetByEmailAsync(request.Email);

            if (existingUser != null)
            {
                throw new ApplicationException(
                    "Email already registered.");
            }

            // DELETE OLD OTP
            var oldOtpList = await _unitOfWork
                .EmailVerificationRepository
                .GetByEmailAsync(request.Email);

            foreach (var item in oldOtpList)
            {
                await _unitOfWork
                    .EmailVerificationRepository
                    .DeleteAsync(item.Id);
            }

            string otp = GenerateOtp();

            var emailVerification =
                new EmailVerification
                {
                    Email = request.Email,

                    Otp = otp,

                    ExpiredAt = DateTime.UtcNow
                        .AddMinutes(10),

                    TemporaryUserData =
                        JsonSerializer.Serialize(request),

                    IsUsed = false,

                    CreatedAt = DateTime.UtcNow
                };

            await _unitOfWork
                .EmailVerificationRepository
                .AddAsync(emailVerification);

            await _unitOfWork.SaveAsync();

            string subject = "Verify your email";

            string message =
                _templateService
                    .GetRegisterOtpTemplate(
                        request.Email,
                        otp);

            // Send email in a background task to prevent blocking the HTTP response and causing Render timeouts
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        request.Email,
                        subject,
                        message);
                }
                catch (Exception ex)
                {
                    // Log error if needed, but do not throw to crash the process
                    Console.WriteLine($"[Email Service Error] Failed to send registration OTP email to {request.Email}: {ex.Message}");
                }
            });

            return true;
        }

        // ========================= VERIFY OTP =========================

        public async Task<string> VerifyOtp(
            RegisterStep2RequestDto request)
        {
            var emailVerification = await _unitOfWork
                .EmailVerificationRepository
                .GetByEmailAndOtpAsync(
                    request.Email,
                    request.Otp);

            if (emailVerification == null ||
                emailVerification.IsUsed ||
                emailVerification.ExpiredAt <
                    DateTime.UtcNow)
            {
                throw new ApplicationException(
                    "Invalid or expired OTP.");
            }

            string verifyToken =
                Guid.NewGuid().ToString();

            emailVerification.VerifyToken =
                verifyToken;

            await _unitOfWork.SaveAsync();

            return verifyToken;
        }

        // ========================= REGISTER STEP 3 =========================

        public async Task<bool> RegisterStep3(
            RegisterStep3RequestDto request)
        {
            var emailVerification = await _unitOfWork
                .EmailVerificationRepository
                .GetByVerifyTokenAsync(
                    request.VerifyToken);

            if (emailVerification == null ||
                emailVerification.IsUsed ||
                emailVerification.ExpiredAt <
                    DateTime.UtcNow ||
                string.IsNullOrEmpty(
                    emailVerification.TemporaryUserData))
            {
                throw new ApplicationException(
                    "Invalid or expired verification token.");
            }

            var step1Data =
                JsonSerializer.Deserialize
                    <RegisterStep1RequestDto>(
                        emailVerification
                            .TemporaryUserData!);

            if (step1Data == null)
            {
                throw new ApplicationException(
                    "Temporary user data not found.");
            }

            string hashedPassword =
                BCrypt.Net.BCrypt.HashPassword(
                    request.Password);

            var defaultRole = await _unitOfWork
                .RoleRepository
                .GetRoleByNameAsync("STUDENT");

            if (defaultRole == null)
            {
                throw new ApplicationException(
                    "Default role not found.");
            }

            var newUser = new User
            {
                UserId = Guid.NewGuid(),

                Email = step1Data.Email,

                Username = step1Data.FullName,

                // Specify DateTimeKind.Utc for DateOfBirth to prevent PostgreSQL "Cannot write DateTime with Kind=Unspecified" exception
                DOB = DateTime.SpecifyKind(step1Data.DateOfBirth, DateTimeKind.Utc),

                Address = step1Data.Address,

                Gender = step1Data.Gender,

                PhoneNumber = step1Data.PhoneNumber,

                PasswordHash = hashedPassword,

                RoleId = defaultRole.RoleId,

                IsActive = true,

                CreateAt = DateTime.UtcNow,

                UpdateAt = DateTime.UtcNow,

                AvatarUrl = ""
            };

            await _unitOfWork.UserRepository
                .AddAsync(newUser);

            emailVerification.IsUsed = true;

            await _unitOfWork.SaveAsync();

            return true;
        }

        // ========================= FORGOT PASSWORD =========================

        public async Task ForgotPassword(
    ForgotPasswordDto dto)
        {
            var user = await _unitOfWork
                .UserRepository
                .GetByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new ApplicationException(
                    "Email does not exist.");
            }

            // DELETE OLD OTP
            var oldOtpList = await _unitOfWork
                .EmailVerificationRepository
                .GetByEmailAsync(dto.Email);

            foreach (var item in oldOtpList)
            {
                await _unitOfWork
                    .EmailVerificationRepository
                    .DeleteAsync(item.Id);
            }

            string otp = GenerateOtp();

            var emailVerification =
                new EmailVerification
                {
                    Email = dto.Email,

                    Otp = otp,

                    ExpiredAt = DateTime.UtcNow
                        .AddMinutes(10),

                    CreatedAt = DateTime.UtcNow,

                    IsUsed = false
                };

            await _unitOfWork
                .EmailVerificationRepository
                .AddAsync(emailVerification);

            await _unitOfWork.SaveAsync();

            string subject = "Reset Password OTP";

            string message =
                _templateService
                    .GetForgotPasswordOtpTemplate(
                        dto.Email,
                        otp);

            // Send email in a background task to prevent blocking the HTTP response and causing Render timeouts
            _ = Task.Run(async () =>
            {
                try
                {
                    await _emailService.SendEmailAsync(
                        dto.Email,
                        subject,
                        message);
                }
                catch (Exception ex)
                {
                    // Log error if needed, but do not throw to crash the process
                    Console.WriteLine($"[Email Service Error] Failed to send forgot password OTP email to {dto.Email}: {ex.Message}");
                }
            });
        }

        // ========================= RESET PASSWORD =========================

        public async Task ResetPassword(
            ResetPasswordDto dto)
        {
            var emailVerification = await _unitOfWork
                .EmailVerificationRepository
                .GetByEmailAndOtpAsync(
                    dto.Email,
                    dto.Otp);

            if (emailVerification == null ||
                emailVerification.IsUsed ||
                emailVerification.ExpiredAt <
                    DateTime.UtcNow)
            {
                throw new ApplicationException(
                    "Invalid or expired OTP.");
            }

            var user = await _unitOfWork
                .UserRepository
                .GetByEmailAsync(dto.Email);

            if (user == null)
            {
                throw new ApplicationException(
                    "User not found.");
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    dto.NewPassword);

            emailVerification.IsUsed = true;

            await _unitOfWork.SaveAsync();
        }

        // ========================= REFRESH TOKEN =========================

        public async Task<LoginResponseDto>
            RefreshToken(string refreshToken)
        {
            var refreshTokenEntity =
                await _unitOfWork
                    .RefreshTokenRepository
                    .GetByTokenAsync(refreshToken);

            if (refreshTokenEntity == null)
            {
                throw new ApplicationException(
                    "Invalid refresh token.");
            }

            if (refreshTokenEntity.RevokedAt != null)
            {
                throw new ApplicationException(
                    "Refresh token revoked.");
            }

            if (refreshTokenEntity.ExpiresAt
                <= DateTime.UtcNow)
            {
                throw new ApplicationException(
                    "Refresh token expired.");
            }

            var user = await _unitOfWork
                .UserRepository
                .GetByIdAsync(
                    refreshTokenEntity.UserId);

            if (user == null)
            {
                throw new ApplicationException(
                    "User not found.");
            }

            var role = await _unitOfWork
                .RoleRepository
                .GetByIdAsync(user.RoleId);

            var roleName = role?.Name ?? "User";

            refreshTokenEntity.RevokedAt =
                DateTime.UtcNow;

            await _unitOfWork
                .RefreshTokenRepository
                .UpdateAsync(refreshTokenEntity);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()),

                new Claim(
                    ClaimTypes.Email,
                    user.Email),

                new Claim(
                    ClaimTypes.Name,
                    user.Username),

                new Claim(
                    ClaimTypes.Role,
                    roleName),

                new Claim(
                    "UserId",
                    user.UserId.ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _configuration["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var accessToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],

                audience: _configuration["Jwt:Audience"],

                claims: claims,

                expires: DateTime.UtcNow
                    .AddMinutes(15),

                signingCredentials: creds
            );

            var accessTokenString =
                new JwtSecurityTokenHandler()
                    .WriteToken(accessToken);

            var newRefreshToken =
                GenerateRefreshToken();

            var newRefreshTokenEntity =
                new RefreshToken
                {
                    RefreshTokenId = Guid.NewGuid(),

                    UserId = user.UserId,

                    TokenHash = BCrypt.Net.BCrypt
                        .HashPassword(newRefreshToken),

                    CreatedAt = DateTime.UtcNow,

                    ExpiresAt = DateTime.UtcNow
                        .AddDays(7),

                    DeviceInfo = "Unknown",

                    IpAddress = "Unknown"
                };

            await _unitOfWork
                .RefreshTokenRepository
                .AddAsync(newRefreshTokenEntity);

            await _unitOfWork.SaveAsync();

            return new LoginResponseDto
            {
                AccessToken = accessTokenString,

                RefreshToken = newRefreshToken
            };
        }

        // ========================= GET ME =========================

        public async Task<MeResponseDto> GetMe(Guid userId)
        {
            var user = await _unitOfWork
                .UserRepository
                .GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApplicationException(
                    "User not found.");
            }

            var response =
                _mapper.Map<MeResponseDto>(user);

            var currentPlanHistory =
                await _unitOfWork
                    .PlanHistoryRepository
                    .GetLatestActiveByUserIdAsync(userId);

            response.CurrentPlan =
                currentPlanHistory?.Plan?.Name ?? "FREE";

            return response;
        }

        // ========================= UPDATE PROFILE =========================

        public async Task<MeResponseDto>
    UpdateProfileAsync(
        Guid userId,
        UpdateProfileDto updateProfileDto)
        {
            var updatedUser =
                await _userService
                    .UpdateProfileAsync(
                        userId,
                        updateProfileDto);

            if (updatedUser == null)
            {
                throw new ApplicationException(
                    "User not found or update failed.");
            }

            var userWithRole =
                await _unitOfWork
                    .UserRepository
                    .GetByIdAsync(userId);

            var response =
                _mapper.Map<MeResponseDto>(
                    userWithRole);

            var currentPlanHistory =
                await _unitOfWork
                    .PlanHistoryRepository
                    .GetLatestActiveByUserIdAsync(userId);

            response.CurrentPlan =
                currentPlanHistory?.Plan?.Name ?? "FREE";

            return response;
        }

        // ========================= HELPER =========================

        private string GenerateOtp()
        {
            Random random = new Random();

            return random
                .Next(100000, 999999)
                .ToString();
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng =
                RandomNumberGenerator.Create();

            rng.GetBytes(randomBytes);

            return Convert.ToHexString(randomBytes);
        }

        public async Task ChangePassword(string email, ChangePasswordDto dto)
        {
            var user = await _unitOfWork
                .UserRepository
                .GetByEmailAsync(email);

            if (user == null)
            {
                throw new ApplicationException(
                    "User not found.");
            }

            bool isCorrectPassword =
                BCrypt.Net.BCrypt.Verify(
                    dto.OldPassword,
                    user.PasswordHash);

            if (!isCorrectPassword)
            {
                throw new ApplicationException(
                    "Old password is incorrect.");
            }

            user.PasswordHash =
                BCrypt.Net.BCrypt.HashPassword(
                    dto.NewPassword);

            user.UpdateAt = DateTime.UtcNow;

            await _unitOfWork.SaveAsync();
        }
    }
}