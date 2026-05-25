using BCrypt.Net;
using BusinessLogic.DTOs;
using BusinessLogic.DTOs.User;
using BusinessLogic.Interfaces;
using DataAccess.Entities;
using DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

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

        public AuthService(IUnitOfWork unitOfWork,
                           IUserService userService,
                           IRoleService roleService,
                           IPlanService planService,
                           IEmailService emailService,
                           IEmailTemplateService templateService,
                           IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
            _roleService = roleService;
            _planService = planService;
            _emailService = emailService;
            _templateService = templateService;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> Login(LoginDto loginDto)
        {
            var user = await _unitOfWork.UserRepository
                .GetByEmailAsync(loginDto.Email);

            // Không nói rõ sai email hay password
            if (user == null ||
                !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new ApplicationException("Invalid email or password.");
            }

            if (!user.IsActive)
            {
                throw new ApplicationException("Account is inactive.");
            }

            var role = await _unitOfWork.RoleRepository
                .GetByIdAsync(user.RoleId);

            var roleName = role?.Name ?? "User";

            // ================= ACCESS TOKEN =================

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Name, user.Username),
        new Claim(ClaimTypes.Role, roleName),
        new Claim("UserId", user.UserId.ToString())
    };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
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

            var accessTokenString = new JwtSecurityTokenHandler()
                .WriteToken(accessToken);

            // ================= REFRESH TOKEN =================

            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                RefreshTokenId = Guid.NewGuid(),
                UserId = user.UserId,
                TokenHash = BCrypt.Net.BCrypt.HashPassword(refreshToken),
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

        public async Task Logout(LogoutDto logoutDto)
        {
            var refreshTokenEntity = await _unitOfWork
                .RefreshTokenRepository
                .GetByTokenAsync(logoutDto.RefreshToken);

            if (refreshTokenEntity == null)
            {
                throw new ApplicationException("Invalid refresh token.");
            }

            refreshTokenEntity.RevokedAt = DateTime.UtcNow;

            await _unitOfWork
                .RefreshTokenRepository
                .UpdateAsync(refreshTokenEntity);

            await _unitOfWork.SaveAsync();
        }
        

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[64];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomBytes);

            return Convert.ToHexString(randomBytes);
        }

        public async Task<bool> RegisterStep1(RegisterStep1RequestDto request)
        {
            // 1. Check if email already exists
            var existingUser = await _unitOfWork.UserRepository.GetByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new ApplicationException("Email already registered.");
            }

            // 2. Generate OTP
            string otp = GenerateOtp();

            // 3. Store user's basic info and OTP temporarily
            var emailVerification = new EmailVerification
            {
                Email = request.Email,
                Otp = otp,
                ExpiredAt = DateTime.UtcNow.AddMinutes(10), // OTP valid for 10 minutes
                TemporaryUserData = JsonSerializer.Serialize(request), // Store step1 data
                IsUsed = false // Ensure it's not marked as used yet
            };
            await _unitOfWork.EmailVerificationRepository.AddAsync(emailVerification);
            await _unitOfWork.SaveAsync();

            // 4. Send OTP to user's email
            //string subject = "Your OTP for Registration";
            //string message = $"Your One-Time Password (OTP) for registration is: <b>{otp}</b>. It is valid for 10 minutes.";

            string subject = "Verify your email";

            string message = _templateService.GetRegisterOtpTemplate(request.Email, otp);

            await _emailService.SendEmailAsync(request.Email, subject, message);

            return true;
        }

        public async Task<string> VerifyOtp(RegisterStep2RequestDto request)
        {
            // 1. Find the email verification record
            var emailVerification = await _unitOfWork.EmailVerificationRepository.GetByEmailAndOtpAsync(request.Email, request.Otp);

            if (emailVerification == null || emailVerification.IsUsed || emailVerification.ExpiredAt < DateTime.UtcNow)
            {
                throw new ApplicationException("Invalid or expired OTP.");
            }

            // 2. Generate verify token
            string verifyToken = Guid.NewGuid().ToString();
            emailVerification.VerifyToken = verifyToken;
            //await _unitOfWork.EmailVerificationRepository.UpdateAsync(emailVerification);
            await _unitOfWork.SaveAsync();

            return verifyToken;
        }

        public async Task<bool> RegisterStep3(RegisterStep3RequestDto request)
        {
            // 1. Validate verify token and retrieve temporary user data
            var emailVerification = await _unitOfWork.EmailVerificationRepository.GetByVerifyTokenAsync(request.VerifyToken);

            if (emailVerification == null || emailVerification.IsUsed || emailVerification.ExpiredAt < DateTime.UtcNow || string.IsNullOrEmpty(emailVerification.TemporaryUserData))
            {
                throw new ApplicationException("Invalid or expired verification token.");
            }

            var step1Data = JsonSerializer.Deserialize<RegisterStep1RequestDto>(emailVerification.TemporaryUserData);
            if (step1Data == null)
            {
                throw new ApplicationException("Temporary user data not found.");
            }

            // 2. Hash the password
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Get default role (STUDENT) and plan (FREE)
            var defaultRole = await _unitOfWork.RoleRepository.GetRoleByNameAsync("STUDENT");
            if (defaultRole == null)
            {
                throw new ApplicationException("Default 'STUDENT' role not found.");
            }

            var defaultPlan = await _unitOfWork.PlanRepository.GetPlanByNameAsync("FREE");
            if (defaultPlan == null)
            {
                throw new ApplicationException("Default 'FREE' plan not found.");
            }

            // 4. Create the actual User entity
            var newUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = step1Data.Email,
                Username = step1Data.FullName, // Mapped FullName to Username
                DOB = step1Data.DateOfBirth, // Mapped DateOfBirth to DOB
                Address = step1Data.Address,
                PhoneNumber = step1Data.PhoneNumber,
                PasswordHash = hashedPassword,
                RoleId = defaultRole.RoleId, // Corrected to RoleId
                PlanId = defaultPlan.PlanId, // Corrected to PlanId
                IsActive = true, // User is active upon registration
                CreateAt = DateTime.UtcNow, // Corrected to CreateAt
                UpdateAt = DateTime.UtcNow, // Added UpdateAt

                AvatarUrl = "", // hoặc ảnh default
                DatePlanRegistration = DateTime.UtcNow
            };

            await _unitOfWork.UserRepository.AddAsync(newUser);
            await _unitOfWork.SaveAsync();

            // 5. Clean up temporary data (optional, can be handled by a background job or retention policy)
            await _unitOfWork.EmailVerificationRepository.DeleteAsync(emailVerification.Id); // Or delete it
            await _unitOfWork.SaveAsync();

            return true;
        }

        private string GenerateOtp()
        {
            Random random = new Random();
            return random.Next(100000, 999999).ToString(); // 6-digit OTP
        }
        public async Task<LoginResponseDto> RefreshToken(
    string refreshToken)
        {
            var refreshTokenEntity = await _unitOfWork
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

            if (refreshTokenEntity.ExpiresAt <= DateTime.UtcNow)
            {
                throw new ApplicationException(
                    "Refresh token expired.");
            }

            var user = await _unitOfWork.UserRepository
                .GetByIdAsync(refreshTokenEntity.UserId);

            if (user == null)
            {
                throw new ApplicationException(
                    "User not found.");
            }

            var role = await _unitOfWork.RoleRepository
                .GetByIdAsync(user.RoleId);

            var roleName = role?.Name ?? "User";

            // REVOKE OLD TOKEN
            refreshTokenEntity.RevokedAt =
                DateTime.UtcNow;

            await _unitOfWork
                .RefreshTokenRepository
                .UpdateAsync(refreshTokenEntity);

            // CREATE ACCESS TOKEN
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

            // CREATE NEW REFRESH TOKEN
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
        public async Task<MeResponseDto> GetMe(Guid userId)
        {
            var user = await _unitOfWork.UserRepository
                .GetByIdAsync(userId);

            if (user == null)
            {
                throw new ApplicationException("User not found.");
            }

            var role = await _unitOfWork.RoleRepository
                .GetByIdAsync(user.RoleId);

            return new MeResponseDto
            {
                UserId = user.UserId,

                Username = user.Username,

                Email = user.Email,

                Address = user.Address,

                PhoneNumber = user.PhoneNumber,

                DOB = user.DOB,

                AvatarUrl = user.AvatarUrl,

                Role = role?.Name ?? "User",

                LastLoginTime = user.LastLoginTime
            };
        }
    }
}
